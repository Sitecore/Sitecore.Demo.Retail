using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Demo.Retail.Feature.Orders.Website.Models;
using Sitecore.Demo.Retail.Feature.Orders.Website.Repositories;
using Sitecore.Demo.Retail.Foundation.Commerce.Website;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Extensions;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models;
using Sitecore.Demo.Retail.Foundation.Commerce.Website.Models.InputModels;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Attributes;

namespace Sitecore.Demo.Retail.Feature.Orders.Website.Controllers
{
    public class OrdersController : Controller
    {
        public OrdersController(OrderManager orderManager, AccountManager accountManager, CommerceUserContext commerceUserContext, OrdersViewModelRepository ordersViewModelRepository, StorefrontContext storefrontContext, OrderViewModelRepository orderViewModelRepository)
        {
            OrderManager = orderManager;
            AccountManager = accountManager;
            CommerceUserContext = commerceUserContext;
            OrdersViewModelRepository = ordersViewModelRepository;
            StorefrontContext = storefrontContext;
            OrderViewModelRepository = orderViewModelRepository;
        }

        private OrderManager OrderManager { get; }
        private AccountManager AccountManager { get; }
        private CommerceUserContext CommerceUserContext { get; }
        public OrdersViewModelRepository OrdersViewModelRepository { get; }
        public StorefrontContext StorefrontContext { get; }
        public OrderViewModelRepository OrderViewModelRepository { get; }


        [HttpGet]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
        public ActionResult MyOrders()
        {
            if (!Context.User.IsAuthenticated)
            {
                return new EmptyResult();
            }

            var orders = OrdersViewModelRepository.Get(CommerceUserContext.Current);
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
        [SkipAnalyticsTracking]
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

                var response = OrderManager.GetUserOrders(CommerceUserContext.Current.UserName);
                var result = new OrdersApiModel(response?.ServiceProviderResult);
                if (response != null)
                {
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
        [SkipAnalyticsTracking]
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

                var response = OrderManager.Reorder(CommerceUserContext.Current.UserId, inputModel);
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
        [SkipAnalyticsTracking]
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

                var response = OrderManager.CancelOrder(CommerceUserContext.Current.UserId, inputModel);
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