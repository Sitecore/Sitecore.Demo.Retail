using System.Collections.Generic;

namespace Sitecore.Foundation.Commerce.Website.Models
{
    public interface ICatalogProduct
    {
        IEnumerable<ICatalogProductVariant> Variants { get; }
        string CatalogName { get; }
        string ProductId { get; }
        string ProductUrl { get; }
        decimal? ListPrice { get; set; }
        decimal? AdjustedPrice { get; set; }
        decimal? LowestPricedVariantAdjustedPrice { get; set; }
        decimal? LowestPricedVariantListPrice { get; set; }
        decimal? HighestPricedVariantAdjustedPrice { get; set; }
    }
}