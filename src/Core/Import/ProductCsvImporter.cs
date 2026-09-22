using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    // Роздільник — крапка з комою: не конфліктує з комою в назвах товарів.
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && (line.StartsWith("id", StringComparison.OrdinalIgnoreCase) || line.StartsWith("type", StringComparison.OrdinalIgnoreCase)))
                continue; // рядок заголовків

            switch (ParseLine(line))
            {
                case ParseProductOk product:
                    products.Add(product.Value);
                    break;

                case ParseWarehouseOk warehouse:
                    warehouses.Add(warehouse.Value);
                    break;

                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new MixedImportResult(products, warehouses, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Товари: P;id;sku;name;unit;quantity
            ["P", _, "", _, _, _] or ["P", _, _, "", _, _]
                => new ParseFailed("SKU або назва товару порожні"),

            ["P", _, _, _, _, var qty] when !int.TryParse(qty, out int q) || q < 0
                => new ParseFailed($"кількість '{qty}' не є невід'ємним числом"),

            ["P", var id, var sku, var name, var unit, var qty]
                => new ParseProductOk(new ProductDto(id, sku, name, unit, int.Parse(qty))),

            // Склади: W;id;name;location або W;id;name
            ["W", "", ..] or ["W", _, "", ..]
                => new ParseFailed("ID або назва складу порожні"),

            ["W", var id, var name, var sku]
                => new ParseWarehouseOk(new WarehouseDto(id, name, sku)),

            ["W", var id, var sku]
                => new ParseWarehouseOk(new WarehouseDto(id, sku)),

            ["P", ..] => new ParseFailed($"очікую 6 колонок для товару (P;id;sku;name;unit;quantity), отримав {parts.Length}"),
            ["W", ..] => new ParseFailed($"очікую 3 або 4 колонки для складу (W;id;name[;location]), отримав {parts.Length}"),
            { Length: < 5 } => new ParseFailed($"очікую 5 колонок або префікс P/W, отримав {parts.Length}"),
            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseProductOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseWarehouseOk(WarehouseDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
