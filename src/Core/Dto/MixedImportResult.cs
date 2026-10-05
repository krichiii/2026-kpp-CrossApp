namespace Core.Dto;

public sealed record MixedImportResult(
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<WarehouseDto> Warehouses,
    IReadOnlyList<Domain.Product> newProducts,
    IReadOnlyList<string> Errors
)
{
    public MixedImportResult(
        IReadOnlyList<ProductDto> products,
        IReadOnlyList<WarehouseDto> warehouses,
        IReadOnlyList<string> errors
    ) : this(products, warehouses, [], errors) { }

    public int Accepted => Products.Count + Warehouses.Count + newProducts.Count;
    public int Skipped => Errors.Count;
    public int Total => Accepted + Skipped;
    public double ErrorPercentage => Total > 0 ? (double)Skipped / Total * 100.0 : 0.0;
}
