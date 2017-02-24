using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;

namespace Sitecore.Foundation.Commerce
{
    public static class Templates
    {
        public static class CountryFolder
        {
            public static readonly ID ID = new ID("{45EAD99F-6344-4AD5-8FB0-205E8C39BD2A}");
        }

        public static class Country
        {
            public static readonly ID ID = new ID("{9086E8E0-55AD-443F-99AF-CFF0F95E7138}");
            public static class Fields
            {
                public static readonly ID CountryCode = new ID("{F8487720-7F38-40F3-A689-C5CA1722B809}");
                public static readonly ID Name = new ID("{2E19E838-DF61-4CD1-ABB7-106945E60901}");
            }
        }

        public static class Region
        {
            public static readonly ID ID = new ID("{F0D5DD44-101A-46F4-81C8-F48A6FF5D17B}");
            public static class Fields
            {
                public static readonly ID RegionCode = new ID("{A178D0CF-2353-4425-A5D4-466861EBC5BE}");
                public static readonly ID Name = new ID("{703310C3-2BC2-4781-93E7-331ABEF7EAAD}");
            }
        }

        public static class HasRating
        {
            public class Fields
            {
                public const string Rating = "Rating";
            }
        }

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

        public static class IncludeInCommerceSearchResults
        {
            public static readonly ID ID = new ID("{D40A2D89-5AA5-4C9F-BA46-1216E95A13F7}");

            public static class Fields
            {
                public static readonly ID DisplayInSearchResults = new ID("{32B3C63B-B71B-4845-A83F-D91E57AEA220}");
            }
        }

        /// <summary>
        /// Default Sitecore Commerce Templates
        /// </summary>
        public static class Commerce
        {
            public static class SearchSettings
            {
                public static readonly ID ID = new ID("{CB5F3E43-EAF7-4EDB-8235-674745D95059}");

                public static class Fields
                {
                    public static readonly ID DefaultBucketQuery = new ID("{AC51462C-8A8D-493B-9492-34D1F26F20F1}");
                    public static readonly ID PersistentBucketFilter = new ID("{C7815F60-96E1-40CB-BB06-B5F833F73B61}");
                }
            }

            public static class NavigationItem
            {
                public static readonly ID ID = new ID("{E55834FB-7C93-44A2-87C0-62BEBA282CED}");

                public static class Fields
                {
                    public static readonly ID CategoryDatasource = new ID("{2882072B-E310-406B-8DD9-B22C9EA4A0F3}");
                }
            }

            public static class DynamicCategory
            {
                public static readonly ID ID = new ID("{6820281F-3BB3-41B4-8C93-7771EEA496D0}");
            }
        }
    }
}