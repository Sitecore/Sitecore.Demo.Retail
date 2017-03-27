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
            AssetRepository.Current.AddScriptFile("/Scripts/knockout-2.3.0.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/knockout.validation-2.0.0.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/debug-knockout.js", ScriptLocation.Head, true);

            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/ajax-helpers.js", ScriptLocation.Head, true);

            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/errorLineDetailViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/errorSummaryViewModel.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Storefront/errorsummary.js", ScriptLocation.Head, true);
        }
    }
}