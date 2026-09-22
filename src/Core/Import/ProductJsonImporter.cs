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

        JsonDocument doc;

        doc = JsonDocument.Parse(json);

        using (doc)
        {
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
                if (TryGetProperty(doc.RootElement, "products", out var prodElem) && prodElem.ValueKind == JsonValueKind.Array)
                {
                    int pIndex = 0;
                    foreach (JsonElement element in prodElem.EnumerateArray())
                    {
                        pIndex++;
                        switch (DeserializeProduct(element, options))
                        {
                            case ParseProductOk product:
                                products.Add(product.Value);
                                break;

                            case ParseFailed failed:
                                errors.Add($"товар {pIndex}: {failed.Reason}");
                                break;
                        }
                    }
                }

                if (TryGetProperty(doc.RootElement, "warehouses", out var whElem) && whElem.ValueKind == JsonValueKind.Array)
                {
                    int wIndex = 0;
                    foreach (JsonElement element in whElem.EnumerateArray())
                    {
                        wIndex++;
                        switch (DeserializeWarehouse(element, options))
                        {
                            case ParseWarehouseOk warehouse:
                                warehouses.Add(warehouse.Value);
                                break;

                            case ParseFailed failed:
                                errors.Add($"склад {wIndex}: {failed.Reason}");
                                break;
                        }
                    }
                }

                if (products.Count == 0 && warehouses.Count == 0 && errors.Count == 0)
                {
                    errors.Add("JSON об'єкт не містить масивів 'products' або 'warehouses'");
                }
            }
            else
            {
                errors.Add($"Очікується масив JSON або об'єкт, отримано {doc.RootElement.ValueKind}");
            }
        }

        return new MixedImportResult(products, warehouses, errors);
    }

    private static ParseOutcome ParseElement(JsonElement element, JsonSerializerOptions options)
    {
        string? type = null;
        if (TryGetProperty(element, "type", out var typeProp))
        {
            type = typeProp.ValueKind == JsonValueKind.String ? typeProp.GetString() : typeProp.GetRawText();
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
        string id = TryGetProperty(element, "id", out var idProp) ? GetStringValue(idProp) : "";
        string sku = TryGetProperty(element, "sku", out var skuProp) ? GetStringValue(skuProp) : "";
        string name = TryGetProperty(element, "name", out var nameProp) ? GetStringValue(nameProp) : "";
        string unit = TryGetProperty(element, "unit", out var unitProp) ? GetStringValue(unitProp) : "";
        string? note = TryGetProperty(element, "note", out var noteProp) && noteProp.ValueKind != JsonValueKind.Null
            ? GetStringValue(noteProp)
            : null;

        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name))
            return new ParseFailed("SKU або назва товару порожні");

        if (!TryGetProperty(element, "quantity", out var qtyProp))
            return new ParseFailed("кількість '' не є невід'ємним числом");

        int quantity;
        if (qtyProp.ValueKind == JsonValueKind.Number)
        {
            if (!qtyProp.TryGetInt32(out quantity) || quantity < 0)
                return new ParseFailed($"кількість '{qtyProp.GetRawText()}' не є невід'ємним числом");
        }
        else if (qtyProp.ValueKind == JsonValueKind.String)
        {
            string qtyStr = qtyProp.GetString() ?? "";
            if (!int.TryParse(qtyStr, out quantity) || quantity < 0)
                return new ParseFailed($"кількість '{qtyStr}' не є невід'ємним числом");
        }
        else
        {
            string raw = qtyProp.ValueKind == JsonValueKind.Null ? "" : qtyProp.GetRawText();
            return new ParseFailed($"кількість '{raw}' не є невід'ємним числом");
        }

        return new ParseProductOk(new ProductDto(id, sku, name, unit, quantity, note));
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

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        if (element.TryGetProperty(name, out value))
            return true;

        foreach (var prop in element.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string GetStringValue(JsonElement prop)
    {
        return prop.ValueKind switch
        {
            JsonValueKind.String => prop.GetString() ?? "",
            JsonValueKind.Null or JsonValueKind.Undefined => "",
            _ => prop.GetRawText()
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseProductOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseWarehouseOk(WarehouseDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
