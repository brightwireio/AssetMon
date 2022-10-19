using AssetMon.Abstractions;
using AssetMon.Grains.Ingestion;
using AssetMon.Grains.Processing;
using AssetMon.Infrastructure.EventStreaming;
using AssetMon.Infrastructure.Persistence;
using AssetMon.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var asConnString = builder.Configuration["blobStoreConnString"];
var ehConnString = builder.Configuration["eventHubConnString"];
var ehEventHubName = builder.Configuration["eventHubName"];
var ehConsumerGroup = builder.Configuration["eventHubConsumerGroup"];
var dbConnString = builder.Configuration["assetDbConnection"];

builder.Services.AddScoped<IBlobService>(o => new AzureBlobService(asConnString));
builder.Services.AddDbContext<AssetMonDbContext>(o =>
{
    o.UseSqlServer(dbConnString, sqlo => sqlo.EnableRetryOnFailure());
});
builder.Services.AddScoped<IAssetMonRepository, AssetMonRepository>();


builder.Host.UseOrleans(siloBuilder =>
{
    int siloPort = 11111;
    int gatewayPort = 22222;
    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_PORTS")))
    {
        var strPorts = Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_PORTS").Split(',');
        if (strPorts.Length >= 2)
        {
            siloPort = int.Parse(strPorts[0]);
            gatewayPort = int.Parse(strPorts[1]);
        }
    }


    siloBuilder
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "Cluster";
            options.ServiceId = "Service";
        })
        .Configure<SiloOptions>(options =>
        {
            options.SiloName = $"silo_{Environment.MachineName}_{Random.Shared.Next(100)}";
        })
        //.ConfigureEndpoints(siloPort: 11_111, gatewayPort: 30_000)
        .ConfigureEndpoints(IPAddress.Parse(Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_IP")), siloPort: siloPort, gatewayPort: gatewayPort, listenOnAnyHostAddress: true)
        .AddAzureTableGrainStorage("PubSubStore", options => options.ConfigureTableServiceClient(asConnString))
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(asConnString))
        .ConfigureApplicationParts(parts =>
        {
            parts.AddApplicationPart(typeof(ConsumerGrain).Assembly).WithReferences();
            parts.AddApplicationPart(typeof(UnitGrain).Assembly).WithReferences();
        })
        .AddAzureBlobGrainStorageAsDefault(opt =>
            {
                opt.ConfigureBlobServiceClient(asConnString);
            })
        .AddEventHubStreams(
            "my-stream-provider",
            (ISiloEventHubStreamConfigurator configurator) =>
            {
                configurator.ConfigureEventHub(builder => builder.Configure(options =>
                {
                    options.ConfigureEventHubConnection(
                        ehConnString,
                        ehEventHubName,
                        ehConsumerGroup);
                }));
                configurator.UseDataAdapter((sp, n) => ActivatorUtilities.CreateInstance<CustomDataAdapter>(sp));
                configurator.UseAzureTableCheckpointer(
                    builder => builder.Configure(options =>
                    {
                        options.ConfigureTableServiceClient(asConnString);
                        options.PersistInterval = TimeSpan.FromSeconds(10);
                    }));
            })
        .UseDashboard(options => options.HideTrace = true).UseLinuxEnvironmentStatistics()
        ;
});

// LOCAL
//builder.Host.UseOrleans(siloBuilder =>
//{
//    siloBuilder
//        .Configure<ClusterOptions>(options =>
//            {
//                options.ClusterId = "Cluster";
//                options.ServiceId = "Service";
//            })
//        .UseLocalhostClustering()
//        .Configure<EndpointOptions>(opts =>
//            {
//                opts.AdvertisedIPAddress = IPAddress.Loopback;
//            })

//        .AddAzureTableGrainStorage("PubSubStore", options => options.ConfigureTableServiceClient(asConnString))
//        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(asConnString))
//        .ConfigureApplicationParts(parts =>
//            {
//                parts.AddApplicationPart(typeof(ConsumerGrain).Assembly).WithReferences();
//                parts.AddApplicationPart(typeof(UnitGrain).Assembly).WithReferences();
//            })
//        .AddAzureBlobGrainStorageAsDefault(opt =>
//            {
//                opt.ConfigureBlobServiceClient(asConnString);
//            })
//        .AddEventHubStreams(
//            "my-stream-provider",
//            (ISiloEventHubStreamConfigurator configurator) =>
//            {
//                configurator.ConfigureEventHub(builder => builder.Configure(options =>
//                {
//                    options.ConfigureEventHubConnection(
//                        ehConnString,
//                        ehEventHubName,
//                        "$Default");

//                }));

//                configurator.UseDataAdapter(
//                    (sp, n) => ActivatorUtilities.CreateInstance<CustomDataAdapter>(sp));
//                configurator.UseAzureTableCheckpointer(
//                    builder => builder.Configure(options =>
//                    {
//                        options.ConfigureTableServiceClient(asConnString);
//                        options.PersistInterval = TimeSpan.FromSeconds(10);
//                    }));
//            });
//            //.AddStartupTask(async (IServiceProvider services, CancellationToken cancellation) =>
//            //    {

//            //        var grainFactory = services.GetRequiredService<IGrainFactory>();
//            //        var grain = grainFactory.GetGrain<IConsumerGrain>(Guid.Parse("3d44df51-8d82-45a8-9794-c42fd3c57229"));
//            //        await grain.Initialize();
//            //    });

//});

builder.Services.AddWebAppApplicationInsights("Silo");

//builder.Services.DontHostGrainsHere();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Ingestion Silo V0.2 - App Service Version :)"));

app.Run();
