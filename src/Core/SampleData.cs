using Core.Domain;

public static class SampleData
{
    public static IEnumerable<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 120),
        Product.Create("P-002", "SKU-002", "Пісок будівельний", "т", 25),
        Product.Create("P-003", "SKU-003", "Цегла керамічна рядова", "шт", 2500),
        Product.Create("P-004", "SKU-004", "Газоблок D500 600x300x200", "шт", 450),
        Product.Create("P-005", "SKU-005", "Арматура А500С 12мм", "м", 800),
        Product.Create("P-006", "SKU-006", "Щебінь гранітний фр. 5-20", "т", 40),
        Product.Create("P-007", "SKU-007", "Кабель UTP cat6 4P", "м", 350),
        Product.Create("P-008", "SKU-008", "Кабель силовий ВВГнг 3х2.5", "м", 500),
        Product.Create("P-009", "SKU-009", "Автоматичний вимикач 16А", "шт", 45),
        Product.Create("P-010", "SKU-010", "Грунтовка глибокого проникнення 10л", "шт", 30),
        Product.Create("P-011", "SKU-011", "Фарба фасадна акрилова 14кг", "шт", 9),
        Product.Create("P-012", "SKU-012", "Шпаклівка фінішна 20кг", "шт", 60),
        Product.Create("P-013", "SKU-013", "Мінеральна вата 100мм", "м2", 150),
        Product.Create("P-014", "SKU-014", "Гіпсокартон вологостійкий 12.5мм", "лист", 85),
        Product.Create("P-015", "SKU-015", "Профіль для гіпсокартону CD-60 3м", "шт", 140),
        Product.Create("P-016", "SKU-016", "Саморізи по металу 3.5х25", "уп", 55),
        Product.Create("P-017", "SKU-017", "Труба поліпропіленова 20мм", "м", 2),
        Product.Create("P-018", "SKU-018", "Піна монтажна всесезонна 750мл", "шт", 35)
    ];

    public static IEnumerable<Movement> Movements() =>
    [
        new("MOV-001", "P-001", 50, MovementType.Incoming, new DateTime(2026, 10, 1), "Постачання від заводу"),
        new("MOV-002", "P-001", 20, MovementType.Outgoing, new DateTime(2026, 10, 5), "Відвантаження замовнику"),
        new("MOV-003", "P-003", 1000, MovementType.Incoming, new DateTime(2026, 10, 2), "Партія №12"),
        new("MOV-004", "P-003", 400, MovementType.Outgoing, new DateTime(2026, 10, 10), "Будівельний майданчик А"),
        new("MOV-005", "P-004", 150, MovementType.Incoming, new DateTime(2026, 10, 3), "Постачання блоків"),
        new("MOV-006", "P-007", 100, MovementType.Outgoing, new DateTime(2026, 10, 12), "Монтажні роботи"),
        new("MOV-007", "P-008", 200, MovementType.Incoming, new DateTime(2026, 10, 4), "Прихід кабелю"),
        new("MOV-008", "P-008", 150, MovementType.Outgoing, new DateTime(2026, 10, 8), "Проєкт Б"),
        new("MOV-009", "P-011", 5, MovementType.Incoming, new DateTime(2026, 10, 15), "Видача клієнту"),
        new("MOV-010", "P-014", 50, MovementType.Incoming, new DateTime(2026, 10, 6), "Постачання"),
        new("MOV-011", "P-014", 30, MovementType.Outgoing, new DateTime(2026, 10, 14), "Ремонт офісу")
    ];

    public static List<Product> Generate(int count)
    {
        var random = new Random(42);
        string[] units = ["шт", "м", "т", "лист", "уп", "м2"];
        var list = new List<Product>(count);
        for (int i = 1; i <= count; i++)
        {
            list.Add(Product.Create(
                $"P-{i:D6}",
                $"SKU-{i:D6}",
                $"Товар #{i}",
                units[random.Next(units.Length)],
                random.Next(0, 500)
            ));
        }
        return list;
    }
}