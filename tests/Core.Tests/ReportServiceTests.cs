namespace Core.Tests;

public class ReportServiceTests
{
    [Theory] [InlineData(10, 2)] [InlineData(1, 0)] // 3) звіт на кількох наборах
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected) =>
        Assert.Equal(expected, ReportService.LowStock(DataTests.Products(), threshold).Count);

    [Fact] // 4) детермінований топ-N з тижня 7
        public void TopByQuantity_IsDeterministic() =>
            Assert.Equal(new[] { "SKU-BIG", "SKU-MID" },
                ReportService.TopByQuantity(DataTests.Products(), 2).Select(l => l.Sku));
}