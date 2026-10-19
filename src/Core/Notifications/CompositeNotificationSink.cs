using Core.Abstractions;

namespace Core.Notifications;

public sealed class CompositeNotificationSink : INotificationSink
{
    private readonly IReadOnlyList<INotificationSink> _sinks;

    public CompositeNotificationSink(IEnumerable<INotificationSink> sinks)
    {
        ArgumentNullException.ThrowIfNull(sinks);
        _sinks = sinks.ToList();
    }

    public CompositeNotificationSink(params INotificationSink[] sinks)
        : this((IEnumerable<INotificationSink>)sinks)
    {
    }

    public void Notify(CatalogChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        foreach (var sink in _sinks)
        {
            sink.Notify(args);
        }
    }
}

