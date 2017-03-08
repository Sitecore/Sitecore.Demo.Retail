// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Habitat
{
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Entitlements;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The Habitat configure class.
    /// </summary>
    /// <seealso cref="Sitecore.Framework.Configuration.IConfigureSitecore" />
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);
            services.RegisterAllCommands(assembly);

            services.Sitecore().Pipelines(
                config => config.ConfigurePipeline<IInitializeEnvironmentPipeline>(
                        d =>
                            {
                                d.Add<InitializeEnvironmentGiftCardsBlock>()
                                    .Add<InitializeEnvironmentSellableItemsBlock>()
                                    .Add<InitializeEnvironmentPricingBlock>()
                                    .Add<InitializeEnvironmentPromotionsBlock>();
                            })

                    .ConfigurePipeline<IProvisionEntitlementsPipeline>(
                        c =>
                            {
                                c.Add<ProvisionWarrantyEntitlementsBlock>().After<ProvisionEntitlementsBlock>()
                                    .Add<ProvisionDigitalProductEntitlementsBlock>().After<ProvisionEntitlementsBlock>()
                                    .Add<ProvisionInstallationEntitlementsBlock>().After<ProvisionEntitlementsBlock>();
                            })

                    .ConfigurePipeline<IGetEntityViewPipeline>(c =>
                        {
                            c.Add<GetOrderDigitalProductEntitlementDetailsViewBlock>().After<Sitecore.Commerce.Plugin.Entitlements.GetOrderEntitlementsViewBlock>()
                             .Add<GetCustomerDigitalProductEntitlementDetailsViewBlock>().After<Sitecore.Commerce.Plugin.Entitlements.GetCustomerEntitlementsViewBlock>()
                             .Add<GetOrderWarrantyEntitlementDetailsViewBlock>().After<Sitecore.Commerce.Plugin.Entitlements.GetOrderEntitlementsViewBlock>()
                             .Add<GetCustomerWarrantyEntitlementDetailsViewBlock>().After<Sitecore.Commerce.Plugin.Entitlements.GetCustomerEntitlementsViewBlock>()
                             .Add<GetOrderInstallationEntitlementDetailsViewBlock>().After<Sitecore.Commerce.Plugin.Entitlements.GetOrderEntitlementsViewBlock>()
                             .Add<GetCustomerInstallationEntitlementDetailsViewBlock>().After<Sitecore.Commerce.Plugin.Entitlements.GetCustomerEntitlementsViewBlock>();
                        })

                    .ConfigurePipeline<IBootstrapPipeline>(d => { d.Add<InitializeEnvironmentHabitatEnsureCatalogBlock>(); }));
        }
    }
}