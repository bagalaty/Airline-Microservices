namespace BuildingBlocks.Outbox;

public class OutboxOptions
{
    public bool Enabled { get; set; } = true;
    public TimeSpan? Interval { get; set; }
    public bool UseBackgroundDispatcher { get; set; } = true;
}
