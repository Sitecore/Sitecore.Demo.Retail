using System;
using Sitecore.Diagnostics;
using Sitecore.Feature.Language.Infrastructure.Pipelines;
using Sitecore.Foundation.Commerce.Website;
using Sitecore.Foundation.Commerce.Website.Managers;

namespace Sitecore.Project.Commerce.Retail.Website.Infrastructure.Pipelines
{
    public class RaiseCommerceCultureChosenPageEvent
    {
        public RaiseCommerceCultureChosenPageEvent(CatalogManager catalogManager, StorefrontContext storefrontContext)
        {
            CatalogManager = catalogManager;
            StorefrontContext = storefrontContext;
        }

        public StorefrontContext StorefrontContext { get; }

        public CatalogManager CatalogManager { get; }

        public void Process(ChangeLanguagePipelineArgs args)
        {
            if (StorefrontContext.Current == null)
            {
                return;
            }

            try
            {
                var result = CatalogManager.RaiseCultureChosenPageEvent(args.NewLanguage);
            }
            catch (Exception e)
            {
                Log.Error("Could not trigger CatalogManager.RaiseCultureChosenPageEvent", e, this);
            }
        }
    }
}