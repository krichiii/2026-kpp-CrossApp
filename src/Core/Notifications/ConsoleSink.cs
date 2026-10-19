using Core.Abstractions;

namespace Core.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var line = $"{args.At:HH:mm:ss} {args.Kind,-8} {args.Id} {args.Delta,4:+0;-0} {args.QuantityAfter}";
        Console.WriteLine(line);
    }
}

