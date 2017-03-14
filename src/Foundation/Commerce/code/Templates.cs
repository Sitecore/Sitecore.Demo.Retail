using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Data;

namespace Sitecore.Foundation.Commerce
{
    public static class Templates
    {
        public static class CurrencyContext
        {
            public static readonly ID ID = new ID("{B0A3F504-6AA6-42B7-99FE-8280D8CC01B1}");

            public static class Fields
            {
                public static readonly ID DefaultCurrency = new ID("{7315350F-F3B5-4740-A227-186F7A2BA9F7}");
            }
        }

        public static class Currency
        {
            public static readonly ID ID = new ID("{BA255355-225E-4EA7-AAF7-C4A05E8FF7A1");

            public static class Fields
            {
                public static readonly ID Description = new ID("{106D1A2D-66E0-429D-B601-07019BECA165}");
                public static readonly ID Symbol = new ID("{083DB23D-64D5-48F7-9F2A-710C5612A4A4}");
                public static readonly ID SymbolPosition = new ID("{21D2DAA4-1FC3-4EA6-B75F-608CB27770FF}");
                public static readonly ID NumberFormatCulture = new ID("{1B863C74-D174-4691-B89A-BF30884B3932}");
            }
        }

        /// <summary>
        ///     Default Sitecore Commerce Templates
        /// </summary>
        public static class Commerce
        {
            public static class SearchSettings
            {
                public static readonly ID Id = new ID("{CB5F3E43-EAF7-4EDB-8235-674745D95059}");

                public static class Fields
                {
                    public static readonly ID DefaultBucketQuery = new ID("{AC51462C-8A8D-493B-9492-34D1F26F20F1}");
                    public static readonly ID PersistentBucketFilter = new ID("{C7815F60-96E1-40CB-BB06-B5F833F73B61}");
                }
            }

            public static class NavigationItem
            {
                public static readonly ID Id = new ID("{E55834FB-7C93-44A2-87C0-62BEBA282CED}");

                public static class Fields
                {
                public static readonly ID CategoryDatasource = new ID("{2882072B-E310-406B-8DD9-B22C9EA4A0F3}");
                }
            }

            public static class DynamicCategory
            {
            public static readonly ID Id = new ID("{6820281F-3BB3-41B4-8C93-7771EEA496D0}");
            }

            public static class CatalogItem
            {
                public static readonly ID Id = new ID("{E55C2650-E1B7-47F7-A725-0DD761B57CCF}");

                public static class Fields
                {
                    public static readonly ID CatalogName = CommerceConstants.KnownFieldIds.CatalogName;
                    public static readonly ID ListPrice = new ID("{2B935D80-96A1-4D69-A1B1-3B518EA7659E}");
                    public static readonly ID DefinitionName = new ID("{221635A5-DB7E-44BC-976A-8D31DACC6025}");
                    public static readonly ID RelationshipList = CommerceConstants.KnownFieldIds.RelationshipList;
                    public static readonly ID PrimaryParentCategory = CommerceConstants.KnownFieldIds.PrimaryParentCategory;
                    public static readonly ID ParentCategories = CommerceConstants.KnownFieldIds.CatalogItemParentCategories;
                }
            }

            public static class Product
            {
                public static readonly ID Id = new ID("{225F8638-2611-4841-9B89-19A5440A1DA1}");

                public static class Fields
                {
                    public static readonly ID Description = new ID("{42022ABE-A882-4EDE-8084-346B13954DA6}");
                    public static readonly ID OnSale = new ID("{4B32C41E-6BAA-40B3-8F75-D471AA07B1ED}");
                    public static readonly ID PriceCardName = new ID("{A4EDC453-9042-41D7-9550-448426A80B2E}");
                    public static readonly ID Rating = new ID("{3A66887E-98FA-4B80-A32E-FA314A2F6205}");
                }
            }

            public static class Category
            {
                public static readonly ID Id = new ID("{4C4FD207-A9F7-443D-B32A-50AA33523661}");

                public static class Fields
                {
                    public static readonly ID ChildProducts = new ID("{95F37041-A3F4-4FA2-8C3C-7A3DB52AAC75}");
                    public static readonly ID ChildCategories = new ID("{90BC8026-5DA2-4AAC-9330-3286CFD80EC7}");
                }
            }

            public static class ProductVariant
            {
                public static readonly ID Id = new ID("{C92E6CD7-7F14-46E7-BBF5-29CE31262EF4}");

                public static class Fields
                {
                public static readonly ID ListPrice = new ID("{9B2ABE41-AB16-463B-8845-A3A5D050A016}");
                }
            }

            public static class Catalog
            {
            public static readonly ID Id = CommerceConstants.KnownTemplateIds.CommerceCatalogTemplate;
            }
        }

        public static class CatalogContext
        {
            public static readonly ID Id = new ID("{25269B92-CC44-46A9-8C77-DA099BF8992C}");

            public static class Fields
            {
                public static readonly ID Catalog = new ID("{B36DE708-9413-4382-BB53-B944E6D02CFC}");
                public static readonly ID CatalogRoot = new ID("{C15A939B-1C97-48E8-B055-37F80DA99C40}");
            }
        }
    }
}