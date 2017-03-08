// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentShopsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.AdventureWorks
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Shops;
    using Sitecore.Commerce.Plugin.Tax;
    using Sitecore.Framework.Pipelines;
    using ManagedLists;

    /// <summary>
    /// Defines a block which bootstraps shops for AdventureWorks Sample environment.
    /// </summary>
    [PipelineDisplayName(AwConstants.Pipelines.Blocks.InitializeEnvironmentShopsBlock)]
    public class InitializeEnvironmentShopsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentShopsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">
        /// The find entity pipeline.
        /// </param>
        public InitializeEnvironmentShopsBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <param name="arg">
        /// The argument.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = "Environment.Shops-1.0";

            //Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>()
                .InitialArtifactSets.Contains(artifactSet))
            {
                return arg;
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={artifactSet}");

            // Default Shop Entity
            await this._persistEntityPipeline.Run(new PersistEntityArgument(
                new Shop
                {
                    Id = $"{CommerceEntity.IdPrefix<Shop>()}Storefront",
                    Name = "Storefront",
                    FriendlyId = "Storefront",
                    DisplayName = "Storefront",
                    //GeoZone = new GeoZone { Latitude = "100", Longitude = "100" },
                    //TimeZone = "GMT-08:00",
                    //Languages = new List<string>
                    //    {
                    //        "en-US",
                    //        "en-CA",
                    //        "fr-FR",
                    //        "de-DE",
                    //        "ja-JP"
                    //    },
                    //DefaultCurrency = "USD",
                    //DefaultCountry = new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA" },
                    //Countries = new List<EntityReference>
                    //    {
                    //        new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA" }
                    //    },
                    //DefaultLanguage = "en-US",
                    //Catalogs = new List<EntityReference>
                    //{
                    //    new EntityReference { EntityTarget = "Adventure Works Catalog" }
                    //},
                    //Policies = new List<Policy>
                    //{
                    //  new GlobalTaxPolicy { PriceIncudesTax = false  },
                    //  new TaxGroupPolicy { TaxGroup = "TaxUsa" }
                    //},
                    Components = new List<Component>
                    {
                        new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<Shop>() } },
                        //new OnlineShopComponent { ServiceUrl="http://localhost:5000" },
                        //new ShopFinancialsComponent {
                        //    LegalEntity = "LegalEntity2",
                        //    DefaultCustomer = "Customer2",
                        //    BusinessUnit = "BusinessUnit2",
                        //    CostCenter = "CostCenter2",
                        //    Department = "Department2" }
                    }
                }
                ), context);

            await this._persistEntityPipeline.Run(new PersistEntityArgument(
                new Shop
                {
                    Id = $"{CommerceEntity.IdPrefix<Shop>()}AwShopCanada",
                    Name = "AwShopCanada",
                    FriendlyId = "AwShopCanada",
                    DisplayName = "Adventure Works Canada",
                    //GeoZone = new GeoZone { Latitude = "100", Longitude = "100" },
                    //TimeZone = "GMT-06:00",
                    //Languages = new List<string>
                    //    {
                    //        "en-CA",
                    //        "fr-FR"
                    //    },
                    //DefaultCurrency = "CAD",
                    //DefaultCountry = new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}CAD" },
                    //Countries = new List<EntityReference>
                    //    {
                    //        new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA" },
                    //        new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}CAN" }
                    //    },
                    //DefaultLanguage = "en-CA",
                    //Catalogs = new List<EntityReference>
                    //{
                    //    new EntityReference { EntityTarget = "Adventure Works Catalog" }
                    //},
                    //Policies = new List<Policy>
                    //{
                    //  new GlobalTaxPolicy { PriceIncudesTax = false  },
                    //  new TaxGroupPolicy { TaxGroup = "TaxCan" }
                    //},
                    Components = new List<Component>
                    {
                        new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<Shop>() } },
                        //new OnlineShopComponent { ServiceUrl="http://localhost:5100" },
                        //new ShopFinancialsComponent {
                        //    LegalEntity = "LegalEntity1",
                        //    DefaultCustomer = "Customer1",
                        //    BusinessUnit = "BusinessUnit1",
                        //    CostCenter = "CostCenter1",
                        //    Department = "Department1" }
                    }
                }
                ), context);

            await this._persistEntityPipeline.Run(new PersistEntityArgument(
                new Shop
                {
                    Id = $"{CommerceEntity.IdPrefix<Shop>()}AwShopUsa",
                    Name = "AwShopUsa",
                    FriendlyId = "AwShopUsa",
                    DisplayName = "Adventure Works USA",
                    //GeoZone = new GeoZone { Latitude = "100", Longitude = "100" },
                    //TimeZone = "GMT-08:00",
                    //Languages = new List<string>
                    //    {
                    //        "en-US"
                    //    },
                    //DefaultCurrency = "USD",
                    //DefaultCountry = new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA" },
                    //Countries = new List<EntityReference>
                    //    {
                    //        new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA" }
                    //    },
                    //DefaultLanguage = "en-US",
                    //Catalogs = new List<EntityReference>
                    //{
                    //    new EntityReference { EntityTarget = "Adventure Works Catalog" }
                    //},
                    //Policies = new List<Policy>
                    //{
                    //  new GlobalTaxPolicy { PriceIncudesTax = false  },
                    //  new TaxGroupPolicy { TaxGroup = "TaxUsa" }
                    //},
                    Components = new List<Component>
                    {
                        new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<Shop>() } },
                        //new OnlineShopComponent { ServiceUrl="http://localhost:5200" },
                        //new ShopFinancialsComponent {
                        //    LegalEntity = "LegalEntity2",
                        //    DefaultCustomer = "Customer2",
                        //    BusinessUnit = "BusinessUnit2",
                        //    CostCenter = "CostCenter2",
                        //    Department = "Department2" }
                    }
                }
                ), context);

            await this._persistEntityPipeline.Run(new PersistEntityArgument(
               new Shop
               {
                   Id = $"{CommerceEntity.IdPrefix<Shop>()}AwShopGermany",
                   Name = "AwShopGermany",
                   DisplayName = "Adventure Works Germany",
                   //GeoZone = new GeoZone { Latitude = "100", Longitude = "100" },
                   //TimeZone = "GMT+01:00",
                   //Languages = new List<string>
                   //    {
                   //         "de-DE",
                   //         "en-US"
                   //    },
                   //DefaultCurrency = "Eur",
                   //DefaultCountry = new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA" },
                   //Countries = new List<EntityReference>
                   //    {
                   //         new EntityReference { EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA" }
                   //    },
                   //DefaultLanguage = "de-DE",
                   //Catalogs = new List<EntityReference>
                   // {
                   //     new EntityReference { EntityTarget = "Adventure Works Catalog" }
                   // },
                   //Policies = new List<Policy>
                   //{
                   //   new GlobalTaxPolicy { PriceIncudesTax = true  },
                   //   new TaxGroupPolicy { TaxGroup = "TaxGermany" }
                   //},
                   Components = new List<Component>
                   {
                       new ListMembershipsComponent { Memberships = new List<string> { CommerceEntity.ListName<Shop>() } },
                        //new OnlineShopComponent { ServiceUrl = "http://localhost:5300" },
                        //new ShopFinancialsComponent {
                        //    LegalEntity = "LegalEntity3",
                        //    DefaultCustomer = "Customer3",
                        //    BusinessUnit = "BusinessUnit3",
                        //    CostCenter = "CostCenter3",
                        //    Department = "Department3"
                        //}
                   }
               }
               ), context);

            return arg;
        }
    }
}
