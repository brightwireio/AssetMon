namespace AssetMon.Infrastructure.EventStreaming;

public class AzureEventHub : IEventHub
{
    private readonly EventHubOptions _options;

    public AzureEventHub(IOptions<EventHubOptions> options)
    {
        _options = options.Value;
    }


    public async Task SendBatch(IEnumerable<object> events)
    {
        var producerClient = new EventHubProducerClient(_options.ConnectionString, _options.EventHub);
        using EventDataBatch eventBatch = await producerClient.CreateBatchAsync(new CreateBatchOptions() { PartitionKey = Guid.NewGuid().ToString() });
        foreach (var @event in events)
        {
            var evt = new EventData(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(@event));
            evt.Properties["StreamNamespace"] = _options.StreamNamespace;
            if (!eventBatch.TryAdd(evt))
            {
                // if it is too large for the batch
                throw new Exception($"Event is too large for the batch and cannot be sent.");
            }
        }

        await producerClient.SendAsync(eventBatch);
        await producerClient.DisposeAsync();
    }
}
