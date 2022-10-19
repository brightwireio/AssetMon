//using Microsoft.Extensions.Logging;
//using Orleans.Streams.Core;
//using Orleans.Streams;
//using Orleans;
//using AssetMon.Abstractions;
//using AssetMon.Common;

//namespace AssetMon.Grains.Ingestion;

////[ImplicitStreamSubscription("ehns-nspoc-orleans")]
//public class ConsumerGrain : Grain, IConsumerGrain, IStreamSubscriptionObserver
//{
//    private readonly ILogger<IConsumerGrain> _logger;
//    private readonly LoggerObserver _observer;

//    /// <summary>
//    /// Class that will log streaming events
//    /// </summary>
//    private class LoggerObserver : IAsyncObserver<UnitEvent>
//    {
//        private readonly ILogger<IConsumerGrain> _logger;
//        private readonly ConsumerGrain _consumerGrain;

//        public LoggerObserver(ILogger<IConsumerGrain> logger, ConsumerGrain grain)
//        {
//            _logger = logger;
//            _consumerGrain = grain;
//        }

//        public Task OnCompletedAsync()
//        {
//            _logger.LogInformation("OnCompletedAsync");
//            return Task.CompletedTask;
//        }

//        public Task OnErrorAsync(Exception ex)
//        {
//            _logger.LogInformation("OnErrorAsync: {Exception}", ex);
//            return Task.CompletedTask;
//        }

//        public Task OnNextAsync(UnitEvent item, StreamSequenceToken? token = null)
//        {
//            var unitGrain = _consumerGrain.GrainFactory.GetGrain<IUnitGrain>(item.VbuNumber);
//            unitGrain.ProcessEvent(item);
//            _logger.LogInformation("OnNextAsync: Stream: {Stream} item: {Item}, token = {Token}", _consumerGrain.GetPrimaryKey(), item, token);
//            return Task.CompletedTask;
//        }
//    }

//    public ConsumerGrain(ILogger<IConsumerGrain> logger)
//    {
//        _logger = logger;
//        _observer = new LoggerObserver(_logger, this);

//        GetStreamProvider("").GetStream<UnitEvent>(Guid.NewGuid(),"").SubscribeAsync( o =>
//        {

//            return Task.CompletedTask;
//        });
//    }

//    // Called when a subscription is added
//    public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
//    {
//        // Plug our LoggerObserver to the stream
//        var handle = handleFactory.Create<UnitEvent>();
//        await handle.ResumeAsync(_observer);
//    }

//    public override Task OnActivateAsync()
//    {
//        _logger.LogInformation("OnActivateAsync");
//        return Task.CompletedTask;
//    }
//}
