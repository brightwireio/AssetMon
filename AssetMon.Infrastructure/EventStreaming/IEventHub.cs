namespace AssetMon.Infrastructure.EventStreaming;

public interface IEventHub
{
    public Task SendBatch(IEnumerable<object> events);

}
