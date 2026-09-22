using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();

        string json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            int index = 0;
            foreach (JsonElement element in doc.RootElement.EnumerateArray())
            {
                index++;
                switch (ParseElement(element, options))
                {
                    case ParseProductOk product:
                        products.Add(product.Value);
                        break;

                    case ParseWarehouseOk warehouse:
                        warehouses.Add(warehouse.Value);
                        break;

                    case ParseFailed failed:
                        errors.Add($"елемент {index}: {failed.Reason}");
                        break;
                }
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            // Підтримка формату { "products": [...], "warehouses": [...] }
            if (doc.RootElement.TryGetProperty("products", out var prodElem) && prodElem.ValueKind == JsonValueKind.Array)
            {
                var pItems = prodElem.Deserialize<List<ProductDto>>(options) ?? [];
                products.AddRange(pItems);
            }

            if (doc.RootElement.TryGetProperty("warehouses", out var whElem) && whElem.ValueKind == JsonValueKind.Array)
            {
                var wItems = whElem.Deserialize<List<WarehouseDto>>(options) ?? [];
                warehouses.AddRange(wItems);
            }

            if (products.Count == 0 && warehouses.Count == 0)
            {
                errors.Add("JSON об'єкт не містить масивів 'products' або 'warehouses'");
            }
        }
        else
        {
            errors.Add($"Очікується масив JSON або об'єкт, отримано {doc.RootElement.ValueKind}");
        }

        return new MixedImportResult(products, warehouses, errors);
    }

    private static ParseOutcome ParseElement(JsonElement element, JsonSerializerOptions options)
    {
        string? type = null;
        if (element.TryGetProperty("type", out var typeProp) || element.TryGetProperty("Type", out typeProp))
        {
            type = typeProp.GetString();
        }

        return type?.ToUpperInvariant() switch
        {
            "P" or "PRODUCT" => DeserializeProduct(element, options),
            "W" or "WAREHOUSE" => DeserializeWarehouse(element, options),

            null => new ParseFailed("не вдалося визначити тип об'єкта (відсутнє поле 'type')"),

            var unknown => new ParseFailed($"невідомий тип об'єкта: '{unknown}'")
        };
    }

    private static ParseOutcome DeserializeProduct(JsonElement element, JsonSerializerOptions options)
    {
        var product = element.Deserialize<ProductDto>(options);
        if (product is null)
           return new ParseFailed("не вдалося десеріалізувати товар");

        if (string.IsNullOrWhiteSpace(product.Sku) || string.IsNullOrWhiteSpace(product.Name))
            return new ParseFailed("SKU або назва товару порожні");
    
        if (product.Quantity < 0)
            return new ParseFailed($"кількість '{product.Quantity}' не є невід'ємним числом");

        return new ParseProductOk(product);
    }

    private static ParseOutcome DeserializeWarehouse(JsonElement element, JsonSerializerOptions options)
    {
        var warehouse = element.Deserialize<WarehouseDto>(options);
        if (warehouse is null)
            return new ParseFailed("не вдалося десеріалізувати склад");

        if (string.IsNullOrWhiteSpace(warehouse.Id) || string.IsNullOrWhiteSpace(warehouse.Sku))
            return new ParseFailed("ID або назва складу порожні");

        return new ParseWarehouseOk(warehouse);

    }

    private abstract record ParseOutcome;
    private sealed record ParseProductOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseWarehouseOk(WarehouseDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
