using AssetMon.Grains.Ingestion;
using AssetMon.Grains.Processing;
using AssetMon.Infrastructure.EventStreaming;
using AssetMon.Infrastructure.Persistence;
using AssetMon.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebAppApplicationInsights("Dashboard");

var asConnString = builder.Configuration.GetValue<string>("StorageConnectionString");
var ehConnString = builder.Configuration["eventHubConnString"];
var ehEventHubName = builder.Configuration["eventHubName"];
var dbConnString = builder.Configuration["assetDbConnection"];

builder.Services.AddScoped<IBlobService>(o => new AzureBlobService(asConnString));
builder.Services.AddDbContext<AssetMonDbContext>(o =>
{
    o.UseSqlServer(dbConnString, sqlo => sqlo.EnableRetryOnFailure());
});
builder.Services.AddScoped<IAssetMonRepository, AssetMonRepository>();




//builder.Host.UseOrleans(siloBuilder =>
//{
//    siloBuilder
//        .Configure<ClusterOptions>(options =>
//        {
//            options.ClusterId = "Cluster";
//            options.ServiceId = "Service";
//        })
//        .Configure<SiloOptions>(options =>
//        {
//            options.SiloName = "Dashboard";
//        })
//        .UseLocalhostClustering()
//        .Configure<EndpointOptions>(opts =>
//        {
//        opts.AdvertisedIPAddress = IPAddress.Loopback;
//        })
//        .AddAzureTableGrainStorage("PubSubStore", options => options.ConfigureTableServiceClient(asConnString))
//        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(asConnString))
//        .ConfigureApplicationParts(parts =>
//        {
//            parts.AddApplicationPart(typeof(UnitGrain).Assembly).WithReferences();
//            parts.AddApplicationPart(typeof(ConsumerGrain).Assembly).WithReferences();
//        })
//        .AddAzureBlobGrainStorageAsDefault(opt =>
//        {
//            opt.ConfigureBlobServiceClient(asConnString);
//        })
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
//            })
//        .UseDashboard( config => config.HideTrace = true);
//});


builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "Cluster";
            options.ServiceId = "Service";
        })
        .Configure<SiloOptions>(options =>
        {
            options.SiloName = "Dashboard";
        })
        .ConfigureEndpoints(siloPort: 11_112, gatewayPort: 30_001)
        .AddAzureTableGrainStorage("PubSubStore", options => options.ConfigureTableServiceClient(asConnString))
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(asConnString))
        //.ConfigureApplicationParts(parts =>
        //{
        //    parts.AddApplicationPart(typeof(ConsumerGrain).Assembly).WithReferences();
        //    parts.AddApplicationPart(typeof(UnitGrain).Assembly).WithReferences();
        //})
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
                        "$Default");
                }));
                configurator.UseDataAdapter(
                    (sp, n) => ActivatorUtilities.CreateInstance<CustomDataAdapter>(sp));
                configurator.UseAzureTableCheckpointer(
                    builder => builder.Configure(options =>
                    {
                        options.ConfigureTableServiceClient(asConnString);
                        options.PersistInterval = TimeSpan.FromSeconds(10);
                    }));
            })
        .UseDashboard( options => options.HideTrace = true);
});

// uncomment this if you dont mind hosting grains in the dashboard
//builder.Services.DontHostGrainsHere();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("DashboardV0.2"));

app.Run();
