using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.Rules;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Demo.Retail.Feature.HabitatData.Engine.Pipelines.Blocks
{
    [PipelineDisplayName(HabitatDataConstants.Pipelines.Blocks.InitializeEnvironmentPromotionsBlock)]
    public class InitializeEnvironmentPromotionsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IAddPromotionBookPipeline _addBookPipeline;
        private readonly IAddPromotionPipeline _addPromotionPipeline;
        private readonly IAddQualificationPipeline _addQualificationPipeline;
        private readonly IAddBenefitPipeline _addBenefitPipeline;
        private readonly IAddPrivateCouponPipeline _addPrivateCouponPipeline;
        private readonly IAddPublicCouponPipeline _addPublicCouponPipeline;
        private readonly IAddPromotionItemPipeline _addPromotionItemPipeline;

        public InitializeEnvironmentPromotionsBlock(
            IPersistEntityPipeline persistEntityPipeline,
            IAddPromotionBookPipeline addBookPipeline,
            IAddPromotionPipeline addPromotionPipeline,
            IAddQualificationPipeline addQualificationPipeline,
            IAddBenefitPipeline addBenefitPipeline,
            IAddPrivateCouponPipeline addPrivateCouponPipeline,
            IAddPromotionItemPipeline addPromotionItemPipeline,
            IAddPublicCouponPipeline addPublicCouponPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
            this._addBookPipeline = addBookPipeline;
            this._addPromotionPipeline = addPromotionPipeline;
            this._addQualificationPipeline = addQualificationPipeline;
            this._addBenefitPipeline = addBenefitPipeline;
            this._addPrivateCouponPipeline = addPrivateCouponPipeline;
            this._addPromotionItemPipeline = addPromotionItemPipeline;
            this._addPublicCouponPipeline = addPublicCouponPipeline;
        }

        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = HabitatDataConstants.ArtifactSets.Promotions;

            // Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>()
                .InitialArtifactSets.Contains(artifactSet))
            {
                return arg;
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={artifactSet}");

            var book =
                await this._addBookPipeline.Run(
                    new AddPromotionBookArgument("Habitat_PromotionBook")
                    {
                        DisplayName = "Habitat Promotion Book",
                        Description = "This is the Habitat promotion book"
                    },
                    context);


            // Line item promotions
            this.CreatePunch360Speaker10DollarffCoupon(book,context);
            this.CreatePunch360Speaker10PctOffCoupon(book,context);
            this.CreateLineOptix25PctOffPromotion(book, context);

            // Cart promotions
            this.CreateCartFreeShippingPromotion(book, context);
            this.CreateCartExclusive5PctOffCouponPromotion(book, context);
            this.CreateCartExclusive5OffCouponPromotion(book, context);
            this.CreateCart15PctOffCouponPromotion(book, context);
            this.CreateCartExclusive10PctOffPromotion(book, context);
            this.CreateCartExclusive65PctBraintreePromotion(book, context);

            return arg;
        }



        #region Cart's Promotions

        private void CreateCartExclusive10PctOffPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
            this._addPromotionPipeline.Run(
                new AddPromotionArgument(book, "Cart 10Pct Off Exclusive Promotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "10% Off Cart over $500 (Exclusive)", "10% Off Cart over $500 (Exclusive)")
                {
                    IsExclusive = true,
                    DisplayName = "10% Off Cart over $500 (Exclusive)",
                    Description = "10% off Cart with subtotal of $500 or more (Exclusive Coupon)"
                },
                context).Result;

            promotion =
            this._addQualificationPipeline.Run(
                new PromotionConditionModelArgument(
                    promotion,
                    new ConditionModel
                    {
                        ConditionOperator = "And",
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                        Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                        Properties = new List<PropertyModel>
                        {
                            new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                            new PropertyModel { Name = "Subtotal", Value = "500", IsOperator = false, DisplayType = "System.Decimal" }
                        }
                    }),
                context).Result;

            promotion =
            this._addBenefitPipeline.Run(
                new PromotionActionModelArgument(
                    promotion,
                    new ActionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                        Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                        Properties = new List<PropertyModel>
                        {
                            new PropertyModel { Name = "PercentOff", Value = "10", DisplayType = "System.Decimal" }
                        }
                    }),
                context).Result;

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }
        /// <summary>
        /// Creates cart free shipping promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateCartFreeShippingPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                this._addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Cart Free Shipping Promotion", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "Free Shipping", "Free Shipping")
                    {
                        DisplayName = "Free Shipping",
                        Description = "Free shipping when Cart subtotal of $100 or more"
                    },
                    context).Result;

            promotion =
                this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                            Name = CartsConstants.Conditions.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                                             {
                                                  new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                  new PropertyModel { Name = "Subtotal", Value = "100", IsOperator = false, DisplayType = "System.Decimal" }
                                             }
                        }),
                    context).Result;

            promotion =
                this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = FulfillmentConstants.Conditions.CartHasFulfillmentCondition,
                            Name = FulfillmentConstants.Conditions.CartHasFulfillmentCondition,
                            Properties = new List<PropertyModel>()
                        }),
                    context).Result;

            this._addBenefitPipeline.Run(
               new PromotionActionModelArgument(
                   promotion,
                   new ActionModel
                   {
                       Id = Guid.NewGuid().ToString(),
                       LibraryId = FulfillmentConstants.Actions.CartFreeShippingAction,
                       Name = FulfillmentConstants.Actions.CartFreeShippingAction
                   }),
               context).Wait();

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates cart exclusive 5 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateCartExclusive5PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
              this._addPromotionPipeline.Run(
                  new AddPromotionArgument(book, "Cart 5Pct Off Coupon Exclusive Promotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "5% Off Cart (Exclusive Coupon)", "5% Off Cart (Exclusive Coupon)")
                  {
                      IsExclusive = true,
                      DisplayName = "5% Off Cart (Exclusive Coupon)",
                      Description = "5% off Cart with subtotal of $10 or more (Exclusive Coupon)"
                  },
                  context).Result;

            promotion =
                this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion =
                this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "5", DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNEC5P"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }
        /// <summary>
        /// Creates a cart exclusive 65% off coupon when the cart subtotal is between $2,000 and $3,000.99.
        /// This is a workaround to the braintree sandbox environment which automatically declines transactions within a certain range.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateCartExclusive65PctBraintreePromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
            this._addPromotionPipeline.Run(
                new AddPromotionArgument(book, "Braintree 65Pct Cart Discount", DateTimeOffset.UtcNow.AddYears(-5), DateTimeOffset.UtcNow.AddYears(10), "Get 65% off your order when the order total is between $2,000 and $3,000.99", "Get 65% off your order when the order total is between $2,000 and $3,000.99")
                {
                    IsExclusive = true,
                    DisplayName = "Braintree 65Pct Cart Discount",
                    Description = "Braintree 65Pct Cart Discount"
                },
                context).Result;

            promotion =
            this._addQualificationPipeline.Run(
                new PromotionConditionModelArgument(
                    promotion,
                    new ConditionModel
                    {
                        ConditionOperator = "And",
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                        Name = CartsConstants.Conditions.CartSubtotalCondition,
                        Properties = new List<PropertyModel>
                        {
                            new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                            new PropertyModel { Name = "Subtotal", Value = "2000", IsOperator = false, DisplayType = "System.Decimal" }
                        }
                    }),
                context).Result;

            promotion =
            this._addQualificationPipeline.Run(
                new PromotionConditionModelArgument(
                    promotion,
                    new ConditionModel
                    {
                        ConditionOperator = "And",
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                        Name = CartsConstants.Conditions.CartSubtotalCondition,
                        Properties = new List<PropertyModel>
                        {
                            new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalLessThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                            new PropertyModel { Name = "Subtotal", Value = "3000.99", IsOperator = false, DisplayType = "System.Decimal" }
                        }
                    }),
                context).Result;

            promotion =
            this._addBenefitPipeline.Run(
                new PromotionActionModelArgument(
                    promotion,
                    new ActionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                        Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                        Properties = new List<PropertyModel>
                        {
                            new PropertyModel { Name = "PercentOff", Value = "65", DisplayType = "System.Decimal" }
                        }
                    }),
                context).Result;

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        /// <summary>
        /// Creates the cart exclusive5 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateCartExclusive5OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
              this._addPromotionPipeline.Run(
                  new AddPromotionArgument(book, "Cart 5 Off Coupon Exclusive Promotion", DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddYears(1), "$5 Off Cart (Exclusive Coupon)", "$5 Off Cart (Exclusive Coupon)")
                  {
                      IsExclusive = true,
                      DisplayName = "$5 Off Cart (Exclusive Coupon)",
                      Description = "$5 off Cart with subtotal of $10 or more (Exclusive Coupon)"
                  },
                  context).Result;

            promotion =
                this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.Conditions.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion =
                this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalAmountOffAction,
                            Name = CartsConstants.Actions.CartSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "AmountOff", Value = "5", DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNEC5A"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

     
        /// <summary>
        /// Creates cart 15 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateCart15PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Cart 15Pct Off Coupon Promotion", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddYears(1), "15% Off Cart (Coupon)", "15% Off Cart (Coupon)")
                   {
                       DisplayName = "15% Off Cart (Coupon)",
                       Description = "15% off Cart with subtotal of $50 or more (Coupon)"
                   },
                   context).Result;

            promotion =
                this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Conditions.CartSubtotalCondition,
                            Name = CartsConstants.Conditions.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "50", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion =
                this._addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "15", DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNC15P"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }


     

    #endregion

    #region Line Promotions
    /// <summary>
    /// Creates line Punch 360 Speaker Promotion (Coupon Code PUNCHD)
    /// </summary>
    /// <param name="book"></param>
    /// <param name="context"></param>
    private void CreatePunch360Speaker10DollarffCoupon(PromotionBook book, CommercePipelineExecutionContext context)
    {
      var promotion =
       this._addPromotionPipeline.Run(
           new AddPromotionArgument(book, "Line Punch 360 10 Off Coupon Excl Promo", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "Punch 360 Speaker 10$ Off Item (Exclusive)", "Punch 360 Speaker 10$ Off Item (Exclusive)")
           {
             DisplayName = "Punch 360 Speaker 10$ Off Item (Exclusive)",
             Description = "10$ off the Punch 360 Speaker item (Exclusive)",
             IsExclusive = true
           },
           context).Result;

      promotion = this._addPromotionItemPipeline.Run(
             new PromotionItemArgument(
                 promotion,
                 "Habitat_Master|6042083|"),
             context).Result;

      this._addBenefitPipeline.Run(
             new PromotionActionModelArgument(
                 promotion,
                 new ActionModel
                 {
                   Id = Guid.NewGuid().ToString(),
                   LibraryId = CartsConstants.Actions.CartItemSubtotalAmountOffAction,
                   Name = CartsConstants.Actions.CartItemSubtotalAmountOffAction,
                   Properties = new List<PropertyModel>
                                      {
                                                  new PropertyModel { Name = "AmountOff", Value = "10", IsOperator = false, DisplayType = "System.Decimal" },
                                                  new PropertyModel { Name = "TargetItemId", Value = "Habitat_Master|6042083|", IsOperator = false, DisplayType = "System.String" }
                                      }
                 }),
             context).Wait();
      promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "PUNCHA"), context).Result;

      promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
      this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
    }
    /// <summary>
    /// Creates line Punch 360 Speaker Promotion (Coupon Code PUNCHD)
    /// </summary>
    /// <param name="book"></param>
    /// <param name="context"></param>
    private void CreatePunch360Speaker10PctOffCoupon(PromotionBook book, CommercePipelineExecutionContext context)
    {
      var promotion =
       this._addPromotionPipeline.Run(
           new AddPromotionArgument(book, "Line Punch 360 10Pct Off Coupon Excl Promo", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "Punch 360 Speaker 10% Off Item (Exclusive)", "Punch 360 Speaker 10% Off Item (Exclusive)")
           {
             DisplayName = "Punch 360 Speaker 10% Off Item (Exclusive)",
             Description = "10% off the Punch 360 Speaker item (Exclusive)",
             IsExclusive = true
           },
           context).Result;

      promotion = this._addPromotionItemPipeline.Run(
             new PromotionItemArgument(
                 promotion,
                 "Habitat_Master|6042083|"),
             context).Result;

      this._addBenefitPipeline.Run(
             new PromotionActionModelArgument(
                 promotion,
                 new ActionModel
                 {
                   Id = Guid.NewGuid().ToString(),
                   LibraryId = CartsConstants.Actions.CartItemSubtotalPercentOffAction,
                   Name = CartsConstants.Actions.CartItemSubtotalPercentOffAction,
                   Properties = new List<PropertyModel>
                                      {
                                                  new PropertyModel { Name = "PercentOff", Value = "10", IsOperator = false, DisplayType = "System.Decimal" },
                                                  new PropertyModel { Name = "TargetItemId", Value = "Habitat_Master|6042083|", IsOperator = false, DisplayType = "System.String" }
                                      }
                 }),
             context).Wait();
      promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "PUNCHP"), context).Result;
      
      promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
      this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
    }


    /// <summary>
    /// Creates line Touch Screen promotion.
    /// </summary>
    /// <param name="book">The book.</param>
    /// <param name="context">The context.</param>
    private void CreateLineOptix25PctOffPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
             this._addPromotionPipeline.Run(
                 new AddPromotionArgument(book, "Line Optix 25Pct Off Promotion", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "25% Off Optix 16.0 Megapixel Digital Camera", "25% Off Optix 16.0 Megapixel Digital Camera")
                 {
                     DisplayName = "Optix 16.0 Megapixel Camera 25% Off",
                     Description = "25% Off Optix 16.0 Megapixel Digital Camera item"
                 },
                 context).Result;

            promotion = this._addPromotionItemPipeline.Run(
                   new PromotionItemArgument(
                       promotion,
                       "Habitat_Master|7042071|"),
                   context).Result;

            this._addBenefitPipeline.Run(
                   new PromotionActionModelArgument(
                       promotion,
                       new ActionModel
                       {
                           Id = Guid.NewGuid().ToString(),
                           LibraryId = CartsConstants.Actions.CartItemSubtotalPercentOffAction,
                           Name = CartsConstants.Actions.CartItemSubtotalPercentOffAction,
                           Properties = new List<PropertyModel>
                                            {
                                                  new PropertyModel { Name = "PercentOff", Value = "25", IsOperator = false, DisplayType = "System.Decimal" },
                                                  new PropertyModel { Name = "TargetItemId", Value = "Habitat_Master|7042071|", IsOperator = false, DisplayType = "System.String" }
                                            }
                       }),
                   context).Wait();

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

    
    #endregion
  }
}
