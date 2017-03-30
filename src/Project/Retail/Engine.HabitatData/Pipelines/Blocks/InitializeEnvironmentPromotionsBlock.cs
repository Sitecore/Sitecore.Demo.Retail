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

namespace Sitecore.Project.Commerce.Engine.Plugin.HabitatData.Pipelines.Blocks
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
            if (arg != "Habitat" && arg != "HabitatShops")
            {
                return arg;
            }

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

            this.CreateCartFreeShippingPromotion(book, context);
            this.CreateCartExclusive5PctOffCouponPromotion(book, context);
            this.CreateCartExclusive5OffCouponPromotion(book, context);
            this.CreateCartExclusiveOptixCameraPromotion(book, context);
            this.CreateCart15PctOffCouponPromotion(book, context);
            this.CreateDisabledPromotion(book, context);

            var date = DateTimeOffset.UtcNow;
            this.CreateCart10PctOffCouponPromotion(book, context, date);
            System.Threading.Thread.Sleep(1); //// TO ENSURE CREATING DATE IS DIFFERENT BETWEEN THESE TWO PROMOTIONS
            this.CreateCart10OffCouponPromotion(book, context, date);

            this.CreateLineTouchScreenPromotion(book, context);
            this.CreateLineTouchScreen5OffPromotion(book, context);
            this.CreateLineExclusiveMiraLaptopPromotion(book, context);
            this.CreateLineExclusive20PctOffCouponPromotion(book, context);
            this.CreateLineExclusive20OffCouponPromotion(book, context);
            this.CreateLine5PctOffCouponPromotion(book, context);
            this.CreateLine5OffCouponPromotion(book, context);

            return arg;
        }

        #region Cart's Promotions

        /// <summary>
        /// Creates cart free shipping promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateCartFreeShippingPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                this._addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "CartFreeShippingPromotion", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "Free Shipping", "Free Shipping")
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
                  new AddPromotionArgument(book, "Cart5PctOffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "5% Off Cart (Exclusive Coupon)", "5% Off Cart (Exclusive Coupon)")
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
        /// Creates the cart exclusive5 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateCartExclusive5OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
              this._addPromotionPipeline.Run(
                  new AddPromotionArgument(book, "Cart5OffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddYears(1), "$5 Off Cart (Exclusive Coupon)", "$5 Off Cart (Exclusive Coupon)")
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
        /// Creates cart exclusive optix camera promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateCartExclusiveOptixCameraPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
             this._addPromotionPipeline.Run(
                 new AddPromotionArgument(book, "CartOptixCameraExclusivePromotion", DateTimeOffset.UtcNow.AddDays(-4), DateTimeOffset.UtcNow.AddYears(1), "Optix Camera 50% Off Cart (Exclusive)", "Optix Camera 50% Off Cart (Exclusive)")
                 {
                     IsExclusive = true,
                     DisplayName = "Optix Camera 50% Off Cart (Exclusive)",
                     Description = "50% off Cart when buying Optix Camera (Exclusive)"
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
                           LibraryId = CartsConstants.Actions.CartSubtotalPercentOffAction,
                           Name = CartsConstants.Actions.CartSubtotalPercentOffAction,
                           Properties = new List<PropertyModel>
                                            {
                                                  new PropertyModel { Name = "PercentOff", Value = "50", IsOperator = false, DisplayType = "System.Decimal" }
                                            }
                       }),
                   context).Wait();

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
                   new AddPromotionArgument(book, "Cart15PctOffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddYears(1), "15% Off Cart (Coupon)", "15% Off Cart (Coupon)")
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

        /// <summary>
        /// Creates the cart10 PCT off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <param name="date">The date.</param>
        private void CreateCart10PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context, DateTimeOffset date)
        {
            var promotion =
               this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Cart10PctOffCouponPromotion", date, date.AddYears(1), "10% Off Cart (Coupon)", "10% Off Cart (Coupon)")
                   {
                       DisplayName = "10% Off Cart (Coupon)",
                       Description = "10% off Cart with subtotal of $50 or more (Coupon)"
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
                                    new PropertyModel { Name = "PercentOff", Value = "10", DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNC10P"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        /// <summary>
        /// Creates the cart10 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <param name="date">The date.</param>
        private void CreateCart10OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context, DateTimeOffset date)
        {
            var promotion =
               this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Cart10OffCouponPromotion", date, date.AddYears(1), "$10 Off Cart (Coupon)", "$10 Off Cart (Coupon)")
                   {
                       DisplayName = "$10 Off Cart (Coupon)",
                       Description = "$10 off Cart with subtotal of $50 or more (Coupon)"
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
                            LibraryId = CartsConstants.Actions.CartSubtotalAmountOffAction,
                            Name = CartsConstants.Actions.CartSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "AmountOff", Value = "10", DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNC10A"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        /// <summary>
        /// Creates the disabled promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateDisabledPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "DisabledPromotion", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), "Disabled", "Disabled")
                   {
                       DisplayName = "Disabled",
                       Description = "Disabled"
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
                                    new PropertyModel { Name = "Subtotal", Value = "5", IsOperator = false, DisplayType = "System.Decimal" }
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
                                    new PropertyModel { Name = "PercentOff", Value = "100", DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion.SetPolicy(new DisabledPolicy());
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context);
        }

        #endregion

        #region Line Promotions

        /// <summary>
        /// Creates line Touch Screen promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateLineTouchScreenPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
             this._addPromotionPipeline.Run(
                 new AddPromotionArgument(book, "LineHabitat34withTouchScreenPromotion", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "Habitat Touch Screen 50% Off", "Habitat Touch Screen 50% Off")
                 {
                     DisplayName = "Habitat Touch Screen 50% Off",
                     Description = "50% off the Habitat 34.0 Cubic Refrigerator with Touchscreen item"
                 },
                 context).Result;

            promotion = this._addPromotionItemPipeline.Run(
                   new PromotionItemArgument(
                       promotion,
                       "Habitat_Master|6042588|"),
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
                                                  new PropertyModel { Name = "PercentOff", Value = "50", IsOperator = false, DisplayType = "System.Decimal" },
                                                  new PropertyModel { Name = "TargetItemId", Value = "Habitat_Master|6042588|", IsOperator = false, DisplayType = "System.String" }
                                            }
                       }),
                   context).Wait();

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        /// <summary>
        /// Creates the line TOuch Scrfeen 5 off promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateLineTouchScreen5OffPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
             this._addPromotionPipeline.Run(
                 new AddPromotionArgument(book, "LineHabitat34withTouchScreen5OffPromotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "Habitat Touch Screen $5 Off Item", "Habitat Touch Screen $5 Off Item")
                 {
                     DisplayName = "Habitat Touch Screen $5 Off",
                     Description = "$5 off the Habitat 34.0 Cubic Refrigerator with Touchscreen item"
                 },
                 context).Result;

            promotion = this._addPromotionItemPipeline.Run(
                   new PromotionItemArgument(
                       promotion,
                       "Habitat_Master|6042588|"),
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
                                                  new PropertyModel { Name = "AmountOff", Value = "5", IsOperator = false, DisplayType = "System.Decimal" },
                                                  new PropertyModel { Name = "TargetItemId", Value = "Habitat_Master|6042588|", IsOperator = false, DisplayType = "System.String" }
                                            }
                       }),
                   context).Wait();

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        /// <summary>
        /// Creates the line mire laptop exclusive promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateLineExclusiveMiraLaptopPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
             this._addPromotionPipeline.Run(
                 new AddPromotionArgument(book, "LineMiraLaptopExclusivePromotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "Mira Laptop 50% Off Item (Exclusive)", "Mira Laptop 50% Off Item (Exclusive)")
                 {
                     DisplayName = "Mira Laptop 50% Off Item (Exclusive)",
                     Description = "50% off the Mira Laptop item (Exclusive)"
                 },
                 context).Result;

            promotion = this._addPromotionItemPipeline.Run(
                   new PromotionItemArgument(
                       promotion,
                       "Habitat_Master|6042179|"),
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
                                                  new PropertyModel { Name = "PercentOff", Value = "50", IsOperator = false, DisplayType = "System.Decimal" },
                                                  new PropertyModel { Name = "TargetItemId", Value = "Habitat_Master|6042179|", IsOperator = false, DisplayType = "System.String" }
                                            }
                       }),
                   context).Wait();

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        /// <summary>
        /// Creates line exclusive 20 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateLineExclusive20PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Line20PctOffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddYears(1), "20% Off Item (Exclusive Coupon)", "20% Off Item (Exclusive Coupon)")
                   {
                       IsExclusive = true,
                       DisplayName = "20% Off Item (Exclusive Coupon)",
                       Description = "20% off any item with subtotal of $50 or more (Exclusive Coupon)"
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
                                    new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
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
                            LibraryId = CartsConstants.Actions.CartAnyItemSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartAnyItemSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "20", DisplayType = "System.Decimal" },
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNEL20P"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        /// <summary>
        /// Creates the line exclusive $20 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateLineExclusive20OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Line20OffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-4), DateTimeOffset.UtcNow.AddYears(1), "$20 Off Item (Exclusive Coupon)", "$20 Off Item (Exclusive Coupon)")
                   {
                       IsExclusive = true,
                       DisplayName = "$20 Off Item (Exclusive Coupon)",
                       Description = "$20 off any item with subtotal of $50 or more (Exclusive Coupon)"
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
                                    new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
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
                            LibraryId = CartsConstants.Actions.CartAnyItemSubtotalAmountOffAction,
                            Name = CartsConstants.Actions.CartAnyItemSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "AmountOff", Value = "20", DisplayType = "System.Decimal" },
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNEL20A"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        /// <summary>
        /// Creates line 5 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateLine5PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Line5PctOffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddYears(1), "5% Off Item (Coupon)", "5% Off Item (Coupon)")
                   {
                       DisplayName = "5% Off Item (Coupon)",
                       Description = "5% off any item with subtotal of 10$ or more (Coupon)"
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
                            LibraryId = CartsConstants.Actions.CartAnyItemSubtotalPercentOffAction,
                            Name = CartsConstants.Actions.CartAnyItemSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "PercentOff", Value = "5", DisplayType = "System.Decimal" },
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNL5P"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        /// <summary>
        /// Creates line 5 amount off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        private void CreateLine5OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
               this._addPromotionPipeline.Run(
                   new AddPromotionArgument(book, "Line5OffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-6), DateTimeOffset.UtcNow.AddYears(1), "$5 Off Item (Coupon)", "$5 Off Item (Coupon)")
                   {
                       DisplayName = "$5 Off Item (Coupon)",
                       Description = "$5 off any item with subtotal of $10 or more (Coupon)"
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
                            LibraryId = CartsConstants.Actions.CartAnyItemSubtotalAmountOffAction,
                            Name = CartsConstants.Actions.CartAnyItemSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                {
                                    new PropertyModel { Name = "AmountOff", Value = "5", DisplayType = "System.Decimal" },
                                    new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Commerce.Plugin.Rules.DecimalGreaterThanOrEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                    new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                }
                        }),
                    context).Result;

            promotion = this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "HABRTRNL5A"), context).Result;
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).Wait();
        }

        #endregion
    }
}
