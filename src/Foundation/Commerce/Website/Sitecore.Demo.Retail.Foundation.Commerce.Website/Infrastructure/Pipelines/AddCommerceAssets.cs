using Sitecore.Foundation.Assets.Models;
using Sitecore.Foundation.Assets.Repositories;
using Sitecore.Mvc.Pipelines.Response.GetPageRendering;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Infrastructure.Pipelines
{
    public class AddCommerceAssets : GetPageRenderingProcessor
    {
        public override void Process(GetPageRenderingArgs args)
        {
            AssetRepository.Current.AddScriptFile("/Scripts/Commerce/knockout-2.3.0.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Commerce/knockout.validation-2.0.0.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Commerce/debug-knockout.js", ScriptLocation.Head, true);
            AssetRepository.Current.AddScriptFile("/Scripts/Commerce/ajax-helpers.js", ScriptLocation.Head, true);
        }
    }
}