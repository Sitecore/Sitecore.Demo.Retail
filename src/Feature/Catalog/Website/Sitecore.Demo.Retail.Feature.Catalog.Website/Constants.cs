namespace Sitecore.Demo.Retail.Feature.Catalog.Website
{
    public class Constants
    {
        public static class QueryString
        {
            public const string SearchKeyword = "q";
            public const string Sort = "s";
            public const string SortDirection = "sd";
            public const string PageSize = "ps";
            public const string Facets = "f";
            public const string Paging = "pg";
        }

        public const char FacetsSeparator = '|';

        public static class IndexFields
        {
            public const string CatalogName = "catalogname";
        }
    }
}