using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

MixedImportResult? result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => null
};

if (result is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу: {Path.GetExtension(path)} (підтримуються тільки .csv та .json)");
    return 1;
}

if (result.Products.Count > 0)
{
    Console.WriteLine($"Завантажено товарів: {result.Products.Count}");
    foreach (ProductDto p in result.Products.Take(5))
        Console.WriteLine($" [P] {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
}

if (result.Warehouses.Count > 0)
{
    Console.WriteLine($"Завантажено складів: {result.Warehouses.Count}");
    foreach (WarehouseDto w in result.Warehouses.Take(5))
        Console.WriteLine($" [W] {w.Id,-6} {w.Sku,-26} {w.Name}");
}

if (result.Products.Count == 0 && result.Warehouses.Count == 0 && result.Errors.Count == 0)
{
    Console.WriteLine("Завантажено записів: 0");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків / елементів: {result.Errors.Count}");
    foreach (string e in result.Errors)
        Console.WriteLine($" ! {e}");
}

Console.WriteLine();
Console.WriteLine($"Статистика імпорту: усього — {result.Total}, прийнято — {result.Accepted}, пропущено — {result.Skipped}, % помилок — {result.ErrorPercentage:F1}%");
 
return 0;
