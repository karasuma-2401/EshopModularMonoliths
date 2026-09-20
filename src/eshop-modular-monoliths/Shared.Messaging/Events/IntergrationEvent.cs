namespace Shared.Messaging.Events;

public record IntergrationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName!;
}