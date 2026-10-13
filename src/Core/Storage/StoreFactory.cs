using Core.Abstractions;
using Core.Domain;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        bool useFile = args.Contains("--file");

        string dataDir = Directory.Exists("data") ? "data" : Path.Combine("..", "..", "data");
        string dataPath = Path.Combine(dataDir, "catalog.json");

        ICatalogStore store = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        return store;
    }
}

