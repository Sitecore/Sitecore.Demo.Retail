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
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/errorsummary_VM.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/MiniCartItemListViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/StoreViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/ConfirmViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/CreditCardPaymentViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/GiftCardPaymentViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/StepIndicator.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/CheckoutDataViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/orders_VM.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/DeliveryAddressViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/CountryRegionViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ViewModels/CartKnockoutModels.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/maps.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/product-details.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/cart.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/order.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/checkout.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/register.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/search.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/editprofile.js", ScriptLocation.Head, true);
        }
    }
}