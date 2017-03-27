using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Orders.Models;
using Sitecore.Feature.Commerce.Orders.Repositories;
using Sitecore.Foundation.Commerce.Extensions;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.Commerce.Models.InputModels;
using Sitecore.Foundation.Commerce.Repositories;

namespace Sitecore.Feature.Commerce.Orders.Controllers
{
    public class OrdersController : Controller
    {
        public OrdersController(OrderManager orderManager, AccountManager accountManager, VisitorContextRepository visitorContextRepository, OrdersViewModelRepository ordersViewModelRepository, StorefrontManager storefrontManager, OrderViewModelRepository orderViewModelRepository)
        {
            OrderManager = orderManager;
            AccountManager = accountManager;
            VisitorContextRepository = visitorContextRepository;
            OrdersViewModelRepository = ordersViewModelRepository;
            StorefrontManager = storefrontManager;
            OrderViewModelRepository = orderViewModelRepository;
        }

        private OrderManager OrderManager { get; }
        private AccountManager AccountManager { get; }
        private VisitorContextRepository VisitorContextRepository { get; }
        public OrdersViewModelRepository OrdersViewModelRepository { get; }
        public StorefrontManager StorefrontManager { get; }
        public OrderViewModelRepository OrderViewModelRepository { get; }


        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult MyOrders()
        {
            if (!Context.User.IsAuthenticated)
            {
                return new EmptyResult();
            }

            var orders = OrdersViewModelRepository.Get(Context.User);
            return View(orders);
        }


        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult MyOrder(string id)
        {
            if (!Context.User.IsAuthenticated)
            {
                return new EmptyResult();
            }

            var order = OrderViewModelRepository.Get(id);
            return View(order);
        }


        [HttpPost]
        [Authorize]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult RecentOrders()
        {
            if (!Context.User.IsAuthenticated)
            {
                Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return null;
            }

            try
            {
                var recentOrders = new List<OrderHeader>();

                var userResponse = AccountManager.GetUser(Context.User.Name);
                var result = new OrdersApiModel(userResponse.ServiceProviderResult);
                if (userResponse.ServiceProviderResult.Success && userResponse.Result != null)
                {
                    var commerceUser = userResponse.Result;
                    var response = OrderManager.GetOrders(commerceUser.ExternalId, StorefrontManager.Current.ShopName);
                    result.SetErrors(response.ServiceProviderResult);
                    if (response.ServiceProviderResult.Success && response.Result != null)
                    {
                        var orders = response.Result.Cast<CommerceOrderHeader>().ToList();
                        recentOrders = orders.Where(order => order.LastModified > DateTime.Today.AddDays(-30)).Take(5).Cast<OrderHeader>().ToList();
                    }
                }

                result.Initialize(recentOrders);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("RecentOrders", e), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult Reorder(ReorderInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));
                var validationResult = this.CreateJsonResult();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = OrderManager.Reorder(StorefrontManager.Current, VisitorContextRepository.GetCurrent(), inputModel);
                var result = new BaseApiModel(response.ServiceProviderResult);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("Reorder", e), JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public JsonResult CancelOrder(CancelOrderInputModel inputModel)
        {
            try
            {
                Assert.ArgumentNotNull(inputModel, nameof(inputModel));
                var validationResult = this.CreateJsonResult<CancelOrderApiModel>();
                if (validationResult.HasErrors)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var response = OrderManager.CancelOrder(StorefrontManager.Current, VisitorContextRepository.GetCurrent(), inputModel);
                var result = new CancelOrderApiModel(response.ServiceProviderResult);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new ErrorApiModel("CancelOrder", e), JsonRequestBehavior.AllowGet);
            }
        }
    }
}