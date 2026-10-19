using Core.Abstractions;

namespace Core.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _filePath;

    public FileSink(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    public void Notify(CatalogChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var line = $"{args.At:HH:mm:ss} {args.Kind,-8} {args.Id} {args.Delta,4:+0;-0} {args.QuantityAfter}";
        File.AppendAllText(_filePath, line + Environment.NewLine);
    }
}

