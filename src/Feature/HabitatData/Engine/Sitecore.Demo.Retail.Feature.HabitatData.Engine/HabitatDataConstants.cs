namespace Sitecore.Demo.Retail.Feature.HabitatData.Engine
{
    public class HabitatDataConstants
    {
        public static class Pipelines
        {
            public static class Blocks
            {
                public const string InitializeEnvironmentPricingBlock = "HabitatData.block.InitializeEnvironmentPricing";
                public const string InitializeEnvironmentPromotionsBlock = "HabitatData.block.InitializeEnvironmentPromotions";
                public const string InitializeEnvironmentGiftCardsBlock = "HabitatData.block.InitializeEnvironmentGiftCards";
                public const string InitializeEnvironmentSellableItemsBlock = "HabitatData.block.InitializeEnvironmentSellableItems";
            }
        }

        public static class ArtifactSets
        {
            public const string Promotions = "Environment.Habitat.Promotions-1.0";
            public const string SellableItems = "Environment.Habitat.SellableItems-1.0";
        }
    }
}
