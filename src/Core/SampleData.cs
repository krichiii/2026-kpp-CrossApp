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
        Product.Create("P-011", "SKU-011", "Фарба фасадна акрилова 14кг", "шт", 22),
        Product.Create("P-012", "SKU-012", "Шпаклівка фінішна 20кг", "шт", 60),
        Product.Create("P-013", "SKU-013", "Мінеральна вата 100мм", "м2", 150),
        Product.Create("P-014", "SKU-014", "Гіпсокартон вологостійкий 12.5мм", "лист", 85),
        Product.Create("P-015", "SKU-015", "Профіль для гіпсокартону CD-60 3м", "шт", 140),
        Product.Create("P-016", "SKU-016", "Саморізи по металу 3.5х25", "уп", 55),
        Product.Create("P-017", "SKU-017", "Труба поліпропіленова 20мм", "м", 200),
        Product.Create("P-018", "SKU-018", "Піна монтажна всесезонна 750мл", "шт", 35)
    ];
}