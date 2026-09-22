namespace Core.Dto;

public sealed record MixedImportResult(
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<WarehouseDto> Warehouses,
    IReadOnlyList<string> Errors
)
{
    public int Accepted => Products.Count + Warehouses.Count;
    public int Skipped => Errors.Count;
    public int Total => Accepted + Skipped;
    public double ErrorPercentage => Total > 0 ? (double)Skipped / Total * 100.0 : 0.0;
}

