// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BootstrapAwEnvironmentBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.AdventureWorks
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Plugin.Sample.Payments.Braintree;
    using Microsoft.Extensions.Logging;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Caching;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Backorderable;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.CsAgent;
    using Sitecore.Commerce.Plugin.Customers.Cs;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Commerce.Plugin.Inventory;
    using Sitecore.Commerce.Plugin.Inventory.Cs;
    using Sitecore.Commerce.Plugin.Journaling;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Management;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Preorderable;
    using Sitecore.Commerce.Plugin.Pricing;
    using Sitecore.Commerce.Plugin.Promotions;
    using Sitecore.Commerce.Plugin.Returns;
    using Sitecore.Commerce.Plugin.SQL;
    using Sitecore.Commerce.Plugin.Tax;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which bootstraps the AdventureWorks environment.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(AwConstants.Pipelines.Blocks.BootstrapAdventureWorksBlock)]
    public class BootstrapAwEnvironmentBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IFindEntityPipeline _findEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="BootstrapAwEnvironmentBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">
        /// The find entity pipeline.
        /// </param>
        /// <param name="findEntityPipeline">
        /// The findEntityPipeline.
        /// </param>
        public BootstrapAwEnvironmentBlock(IPersistEntityPipeline persistEntityPipeline, IFindEntityPipeline findEntityPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
            this._findEntityPipeline = findEntityPipeline;
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
        public override Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            context.Logger.LogInformation($"{this.Name} - Run");

            // Set up initial AdventureWorks environment as an in-memory store
            //var environment = new CommerceEnvironment
            //{
            //    Id = $"{CommerceEntity.IdPrefix<CommerceEnvironment>()}AdventureWorks",
            //    EnvironmentId = new Guid("{0F832C48-694E-4492-94B3-1F196E277299}"),
            //    Name = "AdventureWorks",
            //    Components =
            //                              new List<Component>
            //                                  {
            //                                      new ListMembershipsComponent
            //                                          {
            //                                              Memberships =
            //                                                  new List<string> { CommerceEntity.ListName<CommerceEnvironment>() }
            //                                          }
            //                                  },
            //    Policies =
            //                              new List<Policy>
            //                                  {
            //                                  new EnvironmentInitializationPolicy
            //                                  {
            //                                    InitialArtifactSets = new List<string>
            //                                    {
            //                                        "Carts.CartsGlobalPolicySets-1.0.*",
            //                                        "GiftCards.TestGiftCards-1.0.*",   
            //                                        "Environment.AdventureWorks.PolicySets-1.0.*",                            
            //                                        "Environment.Pricing-1.0.*",
            //                                        "Environment.Promotions-1.0.*",
            //                                        "Environment.Regions-1.0.*",
            //                                        "Environment.SellableItems-1.0.*",
            //                                        "Environment.Shops-1.0.*",
            //                                        "Pricing.DefaultPriceBook-1.0.*"
            //                                    }
            //                                  },
            //                                      new ActionsRolesPolicy(new List<ActionRoleModel>
            //                                                                 {
            //                                                                     //// SNAPSHOTS
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().PriceSnapshotDetails, context.GetPolicy<KnownPricingActionsPolicy>().RequestSnapshotApproval, "sitecore\\Pricer"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().PriceSnapshotDetails, context.GetPolicy<KnownPricingActionsPolicy>().RequestSnapshotApproval, "sitecore\\Pricer Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().PriceSnapshotDetails, context.GetPolicy<KnownPricingActionsPolicy>().RejectSnapshot, "sitecore\\Pricer Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().PriceSnapshotDetails, context.GetPolicy<KnownPricingActionsPolicy>().ApproveSnapshot, "sitecore\\Pricer Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().PriceSnapshotDetails, context.GetPolicy<KnownPricingActionsPolicy>().RetractSnapshot, "sitecore\\Pricer Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().SetSnapshotApprovalStatus, context.GetPolicy<KnownPricingActionsPolicy>().RequestSnapshotApproval, "sitecore\\Pricer"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().SetSnapshotApprovalStatus, context.GetPolicy<KnownPricingActionsPolicy>().RequestSnapshotApproval, "sitecore\\Pricer Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().SetSnapshotApprovalStatus, context.GetPolicy<KnownPricingActionsPolicy>().RejectSnapshot, "sitecore\\Pricer Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().SetSnapshotApprovalStatus, context.GetPolicy<KnownPricingActionsPolicy>().ApproveSnapshot, "sitecore\\Pricer Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPricingViewsPolicy>().SetSnapshotApprovalStatus, context.GetPolicy<KnownPricingActionsPolicy>().RetractSnapshot, "sitecore\\Pricer Manager"),

            //                                                                     //// PROMOTIONS
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().Details, context.GetPolicy<KnownPromotionsActionsPolicy>().RequestPromotionApproval, "sitecore\\Promotioner"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().Details, context.GetPolicy<KnownPromotionsActionsPolicy>().RequestPromotionApproval, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().Details, context.GetPolicy<KnownPromotionsActionsPolicy>().RejectPromotion, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().Details, context.GetPolicy<KnownPromotionsActionsPolicy>().ApprovePromotion, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().Details, context.GetPolicy<KnownPromotionsActionsPolicy>().RetractPromotion, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().Details, context.GetPolicy<KnownPromotionsActionsPolicy>().DisablePromotion, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().SetPromotionApprovalStatus, context.GetPolicy<KnownPromotionsActionsPolicy>().RequestPromotionApproval, "sitecore\\Promotioner"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().SetPromotionApprovalStatus, context.GetPolicy<KnownPromotionsActionsPolicy>().RequestPromotionApproval, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().SetPromotionApprovalStatus, context.GetPolicy<KnownPromotionsActionsPolicy>().RejectPromotion, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().SetPromotionApprovalStatus, context.GetPolicy<KnownPromotionsActionsPolicy>().ApprovePromotion, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().SetPromotionApprovalStatus, context.GetPolicy<KnownPromotionsActionsPolicy>().RetractPromotion, "sitecore\\Promotioner Manager"),
            //                                                                     new ActionRoleModel(context.GetPolicy<KnownPromotionsViewsPolicy>().SetPromotionApprovalStatus, context.GetPolicy<KnownPromotionsActionsPolicy>().DisablePromotion, "sitecore\\Promotioner Manager")
            //                                                                 }),
            //                                      new GlobalEnvironmentPolicy
            //                                          {
            //                                              Languages = new List<string> { "en-US", "en-CA", "es", "fr-CA" },
            //                                              DefaultCurrency = "USD",
            //                                              DefaultCountry =
            //                                                  new EntityReference
            //                                                      {
            //                                                          EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA"
            //                                                      },
            //                                              Countries =
            //                                                  new List<EntityReference>
            //                                                      {
            //                                                          new EntityReference
            //                                                              {
            //                                                                  EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}USA"
            //                                                              },
            //                                                          new EntityReference
            //                                                              {
            //                                                                  EntityTarget = $"{CommerceEntity.IdPrefix<Country>()}CAN"
            //                                                              }
            //                                                      },
            //                                              DefaultLocale = "en-US",
            //                                              TimeZone = "PST",
            //                                              FirstDayOfWeek = "Sunday"
            //                                          },
            //                                      new EntityStoreSqlPolicy
            //                                          {
            //                                              Server = ".",
            //                                              Database = "SitecoreCommerce_SharedEnvironments",
            //                                              TrustedConnection = true,
            //                                              UserName = string.Empty,
            //                                              Password = string.Empty
            //                                          },
            //                                      new InventoryCatalogSqlPolicy
            //                                          {
            //                                              Server = ".",
            //                                              Database = "CFSolutionStorefrontSite_productcatalog",
            //                                              TrustedConnection = true,
            //                                              UserName = string.Empty,
            //                                              Password = string.Empty
            //                                          },
            //                                      new CsCatalogPolicy
            //                                          {
            //                                              SiteName = "CFSolutionStorefrontSite",
            //                                              DebugLevel = "Production",
            //                                              ServiceUrl =
            //                                                  @"http://localhost:1004/CFSolutionStorefrontSite_CatalogWebService/CatalogWebService.asmx",
            //                                              CacheEnable = true,
            //                                              SchemaTimeout = "10",
            //                                              ItemInformationCacheTimeout = "10",
            //                                              ItemHierarchyCacheTimeout = "10",
            //                                              ItemRelationshipsCacheTimeout = "10",
            //                                              ItemAssociationsCacheTimeout = "10",
            //                                              CatalogCollectionCacheTimeout = "10",
            //                                              EnableInventorySystem = true
            //                                          },
            //                                      new CsProfilesPolicy
            //                                          {
            //                                              SiteName = "CFSolutionStorefrontSite",
            //                                              DebugLevel = "Production",
            //                                              PublicKey =
            //                                                  @"registry:HKEY_LOCAL_MACHINE\SOFTWARE\CommerceServer\Encryption\Keys\CSSolutionStorefrontSite,PublicKey",
            //                                              PrivateKey1 =
            //                                                  @"registry:HKEY_LOCAL_MACHINE\SOFTWARE\CommerceServer\Encryption\Keys\CSSolutionStorefrontSite,PrivateKey",
            //                                              KeyIndex = "1"
            //                                          },
            //                                      new ProfilesSqlPolicy
            //                                          {
            //                                              Server = ".",
            //                                              Database = "CFSolutionStorefrontSite_profiles",
            //                                              TrustedConnection = true,
            //                                              UserName = string.Empty,
            //                                              Password = string.Empty
            //                                          },
            //                                      new SitecoreConnectionPolicy
            //                                          {
            //                                              SitecoreDatabase = "master",
            //                                              UserName = "admin",
            //                                              Password = "b",
            //                                              Domain = "sitecore",
            //                                              Host = "cf.reference.storefront.com"
            //                                          },
            //                                      new SitecoreControlPanelItemsPolicy
            //                                          {
            //                                              StorefrontsPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Storefronts",
            //                                              CurrencySetsPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Currency Sets",
            //                                              LanguageSetsPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Language Sets",
            //                                              CommerceTermsLocalizableMessagesPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Commerce Terms/System Messages",
            //                                              CommerceTermsFulfillmentOptionsPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Commerce Terms/Fulfillment Options",
            //                                              CommerceTermsFulfillmentMethodsPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Commerce Terms/Fulfillment Methods",
            //                                              CommerceTermsPaymentOptionsPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Commerce Terms/Payment Options",
            //                                              CommerceTermsPaymentMethodsPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Commerce Terms/Payment Methods",
            //                                              PaymentMethodItemTemplateName = "Federated"
            //                                          },
            //                                      new SitecoreUserTermsPolicy
            //                                          {
            //                                              AccountStatusPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Commerce Terms/CS User Site Terms/Account Status",
            //                                              UserTypePath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Commerce Terms/CS User Site Terms/User Type",
            //                                              AllLanguagesPath =
            //                                                  "/sitecore/Commerce/Commerce Control Panel/Language Sets/All Languages"
            //                                          },
            //                                      new BraintreeClientPolicy
            //                                          {
            //                                              Environment = "sandbox",
            //                                              MerchantId = "ck4y8hkx8wzkrnnw",
            //                                              PublicKey = "wnjjxs5cc98tvzdm",
            //                                              PrivateKey = "ef47369f92cd023013e584a98314b29b"
            //                                          },
            //                                      new RollupCartLinesPolicy { Rollup = true },
            //                                      //// Global caching policy for all entities
            //                                      //// Can be overridden by a specific policy for a specific entity
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "*",
            //                                              AllowCaching = true,
            //                                              Priority = "Normal",
            //                                              Expiration = 60000
            //                                          },
            //                                      //// Dont cache orders
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Orders.Order",
            //                                              AllowCaching = false
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Orders.SalesActivity",
            //                                              AllowCaching = false
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Journaling.JournalEntry",
            //                                              AllowCaching = false
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Fulfillment.Shipment",
            //                                              AllowCaching = false
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Carts.Cart",
            //                                              AllowCaching = false
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.GiftCards.GiftCard",
            //                                              AllowCaching = false
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.ManagedLists.ManagedList",
            //                                              AllowCaching = true,
            //                                              HasNegativeCaching = true,
            //                                              CacheAsEntity = true
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Core.EntityIndex",
            //                                              AllowCaching = true,
            //                                              CacheAsEntity = true
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Promotions.Promotion",
            //                                              AllowCaching = true,
            //                                              CacheAsEntity = true
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Coupons.Coupon",
            //                                              AllowCaching = true,
            //                                              CacheAsEntity = true
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Catalog.SellableItem",
            //                                              AllowCaching = true,
            //                                              CacheAsEntity = true
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Pricing.PriceCard",
            //                                              AllowCaching = true,
            //                                              CacheAsEntity = false
            //                                          },
            //                                      new EntityMemoryCachingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Core.NegativeCache",
            //                                              AllowCaching = true,
            //                                              CacheAsEntity = true
            //                                          },
            //                                      new EntityJournalingPolicy
            //                                          {
            //                                              EntityFullName = "Sitecore.Commerce.Plugin.Orders.Order",
            //                                              Journal = "OrdersJournal"
            //                                          },
            //                                      new RequestRmaReasonsPolicy
            //                                          {
            //                                              List =
            //                                                  new List<RmaReason>
            //                                                      {
            //                                                          new RmaReason
            //                                                              {
            //                                                                  Code = "WrongItem",
            //                                                                  Description = "The wrong item was sent"
            //                                                              },
            //                                                          new RmaReason
            //                                                              {
            //                                                                  Code = "DidNotLike",
            //                                                                  Description = "Customer did not like item"
            //                                                              }
            //                                                      },
            //                                              AllowReturnStatuses = new List<string> { "Completed", "Released" }
            //                                          },
            //                                      new MinionPolicy(
            //                                          "Sitecore.Commerce.Core.NodeHeartBeatMinion, Sitecore.Commerce.Core",
            //                                          string.Empty,
            //                                          TimeSpan.FromSeconds(1)),
            //                                      new MinionBossPolicy(
            //                                          "Sitecore.Commerce.Plugin.Orders.PendingOrdersMinionBoss, Sitecore.Commerce.Plugin.Orders",
            //                                          context.GetPolicy<KnownOrderListsPolicy>().PendingOrders,
            //                                          TimeSpan.FromMinutes(1))
            //                                          {
            //                                              Children = new List<MinionPolicy>
            //                                                             {
            //                                                                 new MinionPolicy(
            //                                                                      "Sitecore.Commerce.Plugin.Orders.PendingOrdersMinion, Sitecore.Commerce.Plugin.Orders",
            //                                                                      $"{context.GetPolicy<KnownOrderListsPolicy>().PendingOrders}.1",
            //                                                                      TimeSpan.FromMinutes(5)),
            //                                                                 new MinionPolicy(
            //                                                                      "Sitecore.Commerce.Plugin.Orders.PendingOrdersMinion, Sitecore.Commerce.Plugin.Orders",
            //                                                                      $"{context.GetPolicy<KnownOrderListsPolicy>().PendingOrders}.2",
            //                                                                      TimeSpan.FromMinutes(5))
            //                                                             }
            //                                          },
            //                                      new MinionPolicy(
            //                                          "Sitecore.Commerce.Plugin.Orders.ReleasedOrdersMinion, Sitecore.Commerce.Plugin.Orders",
            //                                          context.GetPolicy<KnownOrderListsPolicy>().ReleasedOrders,
            //                                          TimeSpan.FromMinutes(5)),
            //                                      new MinionPolicy(
            //                                          "Sitecore.Commerce.Plugin.Backorderable.BackOrdersMinion, Sitecore.Commerce.Plugin.Backorderable",
            //                                          context.GetPolicy<KnownBackorderableListsPolicy>().BackOrders,
            //                                          TimeSpan.FromMinutes(5)),
            //                                      new MinionPolicy(
            //                                          "Sitecore.Commerce.Plugin.Preorderable.PreOrdersMinion, Sitecore.Commerce.Plugin.Preorderable",
            //                                          context.GetPolicy<KnownPreorderableListsPolicy>().PreOrders ,
            //                                          TimeSpan.FromMinutes(5)),
            //                                      new ListNamePolicy { Prefix = "List", Separator = "-", Suffix = "ByDate" },
            //                                      new GlobalOrderPolicy
            //                                          {
            //                                              PolicyId = typeof(GlobalOrderPolicy).Name,
            //                                              InvoicePrefix = "aaa",
            //                                              InvoiceSuffix = "zzz",
            //                                              AllowOrderCancel = true,
            //                                              SubmittedOrderList = "PendingOrders",
            //                                              CompletedOrderList = "CompletedOrders"
            //                                          },
            //                                      new OnHoldOrdersPolicy
            //                                          {
            //                                              AllowHoldStatuses = new List<string> { "Pending", "Problem" }
            //                                          },
            //                                      new CancelOrdersPolicy
            //                                          {
            //                                              AllowCancelStatuses =
            //                                                  new List<string> { "Pending", "Problem", "OnHold" }
            //                                          },
            //                                      new GlobalInventoryPolicy
            //                                          {
            //                                              PolicyId = typeof(GlobalInventoryPolicy).Name,
            //                                              CheckStockLevel = true,
            //                                              SubtractStockOnOrder = true,
            //                                              DefaultAllocationDateAvailable = DateTimeOffset.MinValue
            //                                          },
            //                                      new GlobalPhysicalFulfillmentPolicy
            //                                          {
            //                                              PolicyId = typeof(GlobalPhysicalFulfillmentPolicy).Name,
            //                                              MaxShippingWeight = 50,
            //                                              MeasurementUnits = "Inches",
            //                                              WeightUnits = "Lbs",
            //                                              DefaultCartFulfillmentFee = new Money("USD", 3M),
            //                                              DefaultCartFulfillmentFees =
            //                                                  new List<Money>() { new Money("USD", 10M), new Money("CAD", 12M) },
            //                                              DefaultItemFulfillmentFee = new Money("USD", 3M),
            //                                              DefaultItemFulfillmentFees =
            //                                                  new List<Money>() { new Money("USD", 2M), new Money("CAD", 3M) },
            //                                              FulfillmentFees =
            //                                                  new List<FulfillmentFee>
            //                                                      {
            //                                                          new FulfillmentFee
            //                                                              {
            //                                                                  Name = "Ground",
            //                                                                  Fee = new Money("USD", 15)
            //                                                              },
            //                                                          new FulfillmentFee
            //                                                              {
            //                                                                  Name = "Standard",
            //                                                                  Fee = new Money("USD", 2)
            //                                                              },
            //                                                          new FulfillmentFee
            //                                                              {
            //                                                                  Name = "Next Day Air",
            //                                                                  Fee = new Money("USD", 5)
            //                                                              },
            //                                                          new FulfillmentFee
            //                                                              {
            //                                                                  Name = "Standard Overnight",
            //                                                                  Fee = new Money("USD", 10)
            //                                                              },
            //                                                          new FulfillmentFee
            //                                                              {
            //                                                                  Name = "Ground",
            //                                                                  Fee = new Money("CAD", 15)
            //                                                              },
            //                                                          new FulfillmentFee
            //                                                              {
            //                                                                  Name = "Standard",
            //                                                                  Fee = new Money("CAD", 2)
            //                                                              },
            //                                                          new FulfillmentFee
            //                                                              {
            //                                                                  Name = "Next Day Air",
            //                                                                  Fee = new Money("CAD", 5)
            //                                                              },
            //                                                          new FulfillmentFee
            //                                                              {
            //                                                                  Name = "Standard Overnight",
            //                                                                  Fee = new Money("CAD", 10)
            //                                                              }
            //                                                      }
            //                                          },
            //                                      new GlobalCheckoutPolicy
            //                                          {
            //                                              PolicyId = typeof(GlobalCheckoutPolicy).Name,
            //                                              EnableGuestCheckout = true,
            //                                              EnableQuickCheckout = true,
            //                                              EnableTermsAndConditions = true,
            //                                              MinimumOrderQuantity = 1
            //                                          },
            //                                      new GlobalTaxPolicy { DefaultCartTaxRate = .1M, DefaultItemTaxRate = .1M },
            //                                      new GeoLocationDefaultsPolicy
            //                                          {
            //                                              AreaCode = "613",
            //                                              IpAddress = "127.0.0.1"
            //                                          },
            //                                  }
            //};

            //var existingEnvironment = this._findEntityPipeline.Run(new FindEntityArgument(typeof(CommerceEnvironment), environment.Id), context).Result as CommerceEnvironment;
            //if (existingEnvironment != null)
            //{
            //    environment.IsPersisted = true;
            //}

            //try
            //{
            //    this._persistEntityPipeline.Run(new PersistEntityArgument(environment), context).Wait();
            //}
            //catch (System.Exception ex)
            //{
            //    context.Logger.LogError($"Exception in BootstrapAwEnvironmentBlock - {ex}");
            //    throw;
            //}

            // Set up Azure variant of AdventureWorks Environment
            //var environmentAzure = environment.Clone<CommerceEnvironment>();
            //environmentAzure.Id = $"{CommerceEntity.IdPrefix<CommerceEnvironment>()}AdventureWorksSQLAzure";
            //((CommerceEnvironment)environmentAzure).EnvironmentId = new Guid("{F0C171B7-6F2B-46FE-B2F0-979ED9B00897}");
            //environmentAzure.Name = "AdventureWorksSQLAzure";
            //environmentAzure.Policies.Remove(environmentAzure.GetPolicy<EntityStoreSqlPolicy>());
            //environmentAzure.Policies.Add(new EntityStoreSqlPolicy { Server = "sc821dev01.database.windows.net", TrustedConnection = false, UserName = "ctpuser", Password = "Sitec0re123!" });

            //existingEnvironment = this._findEntityPipeline.Run(new FindEntityArgument(typeof(CommerceEnvironment), environmentAzure.Id), context).Result as CommerceEnvironment;
            //if (existingEnvironment != null)
            //{
            //    environmentAzure.IsPersisted = true;
            //}

            //this._persistEntityPipeline.Run(new PersistEntityArgument(environmentAzure), context).Wait();

            return Task.FromResult(arg);
        }
    }
}
