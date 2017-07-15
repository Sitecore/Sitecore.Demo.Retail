using Sitecore.Data;

namespace Sitecore.Demo.Retail.Feature.Catalog.Website
{
    public static class Templates
    {
        public static class NamedSearch
        {
            public static readonly ID ID = new ID("{F3C0CD6C-9FA9-442D-BD3A-5A25E292F2F7}");

            public static class Fields
            {
                public static readonly ID Title = new ID("{439CDD2A-66A9-4D67-913A-5B696327C867}");
            }
        }

        public static class SelectedProducts
        {
            public static readonly ID ID = new ID("{A45D0030-79F2-4DBF-9A74-226A33C58249}");

            public static class Fields
            {
                public static readonly ID Title = new ID("{357469EE-6674-432F-9B43-EF3534763773}");
                public static readonly ID ProductList = new ID("{75DDF936-9B45-4EBA-88BB-783C9D95486E}");
                
            }
        }

        public static class HasNamedSearches
        {
            public static readonly ID ID = new ID("{657236AE-6950-4516-AA9B-293F8C351D39}");

            public static class Fields
            {
                public static readonly ID NamedSearches = new ID("{BB8B5123-FD0C-48DF-B908-B59C6D67D9CF}");
            }
        }

        //Contains the fields and template names for generated commerce catalog templates
        public class Generated
        {
            public static class Category
            {
                public static class Fields
                {
                    public static readonly string Description = "Description";
                    public static readonly string Images = "Images";
                }
            }

            public static class Product
            {
                public static class Fields
                {
                    public static readonly string Images = "Images";
                    public static readonly string Description = "Description";
                    public static readonly string Brand = "Brand";
                    public static readonly string Tags = "Tags";
                }
            }
        }

        public class ProductOverlayImages
        {
            public static readonly ID ID = new ID("{D4859FED-CD3B-45A9-9CDB-A061809D5B50}");

            public static class Fields
            {
                public static readonly ID AlmostOutOfStockLimit = new ID("{84AA59B2-090D-483C-B3AC-23E68C3CE2EB}");
                public static readonly ID AlmostOutOfStockOverlayImage = new ID("{5E667B19-3D2D-4854-A959-5E4339339817}");
                public static readonly ID OnSaleOverlayImage = new ID("{CD92DFF9-AE15-443C-8DE5-661D23B5AC6C}");
                public static readonly ID OutOfStockOverlayImage = new ID("{A5EB87FB-5D87-40CA-84D1-9AE2C8169A66}");
                public static readonly ID PreorderOverlayImage = new ID("{5BF6C285-0BC0-4A3B-B248-A2355D42AC06}");
            }
        }
    }
}