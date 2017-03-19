using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Foundation.Assets.Models;
using Sitecore.Foundation.Assets.Repositories;
using Sitecore.Mvc.Pipelines.Response.GetPageRendering;

namespace Sitecore.Reference.Storefront.Infrastructure.SitecorePipelines
{
    public class AddCommerceAssets : GetPageRenderingProcessor
    {
        public override void Process(GetPageRenderingArgs args)
        {
            AssetRepository.Current.AddScriptFile("/Scripts/jquery.cookie.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/jquery.unobtrusive-ajax.min.js", ScriptLocation.Head, true);

            AssetRepository.Current.AddScriptFile("/Scripts/jsuri-1.1.1.min.js", ScriptLocation.Head, true);

            AssetRepository.Current.AddScriptFile("/Scripts/knockout-2.3.0.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/knockout.validation-2.0.0.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/debug-knockout.js", ScriptLocation.Head, true);

            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/main.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/adjustmentViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/checkoutDataViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/confirmViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/countryRegionViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/creditCardPaymentViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/deliveryAddressViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/errorLineDetailViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/errorSummaryViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/giftCardPaymentViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/lineItemDataViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/lineItemListViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/miniCartItemListViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/miniCartViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/orderHeaderModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/orderHeaderViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/priceInfoViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/stockInfoListViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/stockInfoViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/Models/storeViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/cart.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/checkout.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/errorsummary.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/maps.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/minicart.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/order.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/orders.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/product-details.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/product-list.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/step-indicator.js", ScriptLocation.Head, true);
        }
    }
}