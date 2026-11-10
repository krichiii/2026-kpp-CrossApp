using System.Diagnostics;
using Core.Abstractions;
using Core.Domain;
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

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
 // для демонстрації таймауту: cts.CancelAfter(TimeSpan.FromMilliseconds(2));
try
{
    var added = 0;
    await foreach (var item in service.ImportStreamAsync(new JsonCatalogImporter(), dataPath, cts.Token))
    {
        added++;
    }
    Console.WriteLine($"Імпортовано записів: {added}");
}
catch (OperationCanceledException) { Console.WriteLine("Імпорт скасовано."); }
catch (DomainException ex) { Console.WriteLine($"Дані відхилено: {ex.Message}"); }
catch (FileNotFoundException) { Console.WriteLine($"Файл не знайдено: {dataPath}"); }