using Core.Abstractions;
using Core.Notifications;

var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "app.log");
var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "catalog.json");
// var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "non-existant.json");
// var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "catalog-error.json");

INotificationSink sink = new CompositeNotificationSink(
    new ConsoleSink(),
    new FileSink(logPath)
);

ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store, sink);

using var userCts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; userCts.Cancel(); };

// Таймаут для тривалих операцій (наприклад, 5 секунд або менше для тесту)
using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
// using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));

// Зв'язаний токен: об'єднує сигнал користувача (Ctrl+C) і автоматичний таймаут
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

try
{
    var added = 0;
    await foreach (var item in service.ImportStreamAsync(new JsonCatalogImporter(), dataPath, linkedCts.Token))
    {
        added++;
    }
    Console.WriteLine($"Імпортовано записів: {added}");
}
catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
{
    Console.WriteLine("Імпорт перервано за таймаутом.");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Імпорт скасовано користувачем.");
}
catch (DomainException ex) { Console.WriteLine($"Дані відхилено: {ex.Message}"); }
catch (FileNotFoundException) { Console.WriteLine($"Файл не знайдено: {dataPath}"); }