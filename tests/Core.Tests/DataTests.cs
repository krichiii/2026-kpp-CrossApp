using Core.Domain;

public static class DataTests
{
    public static IEnumerable<Product> Products() =>
    [
        Product.Create("P-001", "SKU-BIG", "Товар з великим залишком", "шт", 100),
        Product.Create("P-002", "SKU-MID", "Товар із середнім залишком", "шт", 50),
        Product.Create("P-003", "SKU-NORM", "Товар зі звичайним залишком", "шт", 20),
        Product.Create("P-004", "SKU-LOW1", "Товар із малим залишком 1", "шт", 5),
        Product.Create("P-005", "SKU-LOW2", "Товар із малим залишком 2", "шт", 8)
    ];
}
