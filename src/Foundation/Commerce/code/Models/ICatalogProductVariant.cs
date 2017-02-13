namespace Sitecore.Foundation.Commerce.Models
{
    public interface ICatalogProductVariant : IProductVariant
    {
        decimal? ListPrice { get; set; }
        decimal? AdjustedPrice { get; set; }
    }
}