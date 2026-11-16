using System.Reflection;
using Core.Configuration;
using Core.Storage;
using Microsoft.Extensions.Configuration;

var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();

var legacy = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
var storage = new StorageOptions
{
    // проста змінна CROSSAPP_DATA_PATH перекриває розділ Storage
    DataPath = string.IsNullOrWhiteSpace(legacy) ? options.Storage.DataPath : legacy,
    FileName = options.Storage.FileName
};

var resolvedPath = storage.ResolveFullPath(AppContext.BaseDirectory);
var apiKey = config["Integration:ApiKey"];

Console.WriteLine($"Файл даних : {resolvedPath}");
Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");

var store = new FileCatalogStore(storage, AppContext.BaseDirectory);
var service = new CatalogService(store);

if (args.Contains("export", StringComparer.OrdinalIgnoreCase) || args.Contains("--export", StringComparer.OrdinalIgnoreCase))
{
    foreach (var item in store.List())
    {
        store.Remove(item.Id);
    }

    service.Add("A-100", "Цемент М400", "мішок", 40);
    service.Add("A-200", "Пісок будівельний", "т", 25);
    service.Add("A-300", "Цегла червона", "шт", 1500);

    Console.WriteLine($"Експортовано 3 записи.");
}
else
{
    Console.WriteLine($"Записів у сховищі: {store.List().Count}");
}
