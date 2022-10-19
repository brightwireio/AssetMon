namespace AssetMon.Infrastructure.EventStreaming;

public class EventHubOptions
{
    public string ConnectionString { get; set; }

    public string EventHub { get; set; }

    public string StreamNamespace { get; set; }
}
