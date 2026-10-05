using Core.Domain;
using Core.Dto;

var newProducts = new List<Product>();
var errors = new List<string>();

Console.WriteLine("=== Сценарій 1: успіх ===");

Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Product product2 = Product.Create("P-002", "sku-002", "Пісок будівельний", "т", 18);

newProducts.Add(product);
newProducts.Add(product2); //дод завдання 1

Console.WriteLine(product);
product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product);
Console.WriteLine(product2);

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("порожній SKU", () => Product.Create("P-002", " ", "Пісок", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));
TryDo("порожня назва", () => Product.Create("P-003", "SKU-003", "", "шт", -5));

MixedImportResult result = new(
    Products: [],
    Warehouses: [],
    newProducts: newProducts,
    Errors: errors
);

Console.WriteLine();
Console.WriteLine($"Статистика імпорту: усього — {result.Total}, прийнято — {result.Accepted}, пропущено — {result.Skipped}, % помилок — {result.ErrorPercentage:F1}%");

void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        errors.Add($"{title}: {ex.Message}");
        Console.WriteLine($" {title}: {ex.GetType().Name} — {ex.Message}");
    }
}