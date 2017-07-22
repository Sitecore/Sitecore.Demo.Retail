namespace Foundation.Commerce.Website.Models
{
    public interface ICatalogProductVariant : IProductVariant
    {
        decimal? ListPrice { get; set; }
        decimal? AdjustedPrice { get; set; }
    }
}