using System.Runtime.CompilerServices;
using Core.Domain;

namespace Core.Tests;

public class ImportTests
{
    [Fact] // 5) скасування
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new CatalogService(new InMemoryCatalogStore());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    [Fact]
    public async Task ImportStreamAsync_WhenCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new CatalogService(new InMemoryCatalogStore());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in service.ImportStreamAsync(new JsonCatalogImporter(), "any.json", cts.Token))
            {
            }
        });
    }

    [Fact]
    public async Task ImportAsync_WithLinkedTokens_WhenTimeoutExpires_ProvesTimeoutTriggered()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        var service = new CatalogService(new InMemoryCatalogStore());
        var slowImporter = new DelayedCatalogImporter(delayMs: 3000);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(slowImporter, "any.json", linkedCts.Token));

        // Доведення, що спрацював саме таймаут:
        Assert.True(timeoutCts.IsCancellationRequested, "Повинен спрацювати токен таймауту");
        Assert.False(userCts.IsCancellationRequested, "Користувацький токен НЕ повинен бути скасованим");
    }

    [Fact]
    public async Task ImportAsync_WithLinkedTokens_WhenUserCancels_ProvesUserTriggered()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        var service = new CatalogService(new InMemoryCatalogStore());
        var slowImporter = new DelayedCatalogImporter(delayMs: 3000);

        await userCts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(slowImporter, "any.json", linkedCts.Token));

        // Доведення, що спрацювало саме користувацьке скасування:
        Assert.False(timeoutCts.IsCancellationRequested, "Таймаут НЕ повинен був спрацювати");
        Assert.True(userCts.IsCancellationRequested, "Повинен спрацювати користувацький токен");
    }

    private sealed class DelayedCatalogImporter(int delayMs) : ICatalogImporter
    {
        public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
        {
            await Task.Delay(delayMs, ct);
            return [];
        }

        public async IAsyncEnumerable<Product> LoadStreamAsync(
            string path, 
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.Delay(delayMs, ct);
            yield break;
        }
    }
}
