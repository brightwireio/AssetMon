using AssetMon.Grains.Processing;
using AssetMon.Infrastructure.Persistence;
using AssetMon.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var asConnString = builder.Configuration["blobStoreConnString"];

builder.Services.AddScoped<IBlobService>(o => new AzureBlobService(asConnString));
builder.Services.AddDbContext<AssetMonDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("assetDbConnection"), sqlo => sqlo.EnableRetryOnFailure());
});
builder.Services.AddScoped<IAssetMonRepository, AssetMonRepository>();



//builder.Host.UseOrleans(siloBuilder =>
//{
//    siloBuilder
//        .UseLocalhostClustering()
//        .Configure<ClusterOptions>(options =>
//        {
//            options.ClusterId = "Cluster";
//            options.ServiceId = "Service";
//        })
//        .Configure<EndpointOptions>(opts =>
//        {
//            opts.AdvertisedIPAddress = IPAddress.Loopback;
//        })
//        .ConfigureEndpoints(siloPort: 11_113, gatewayPort: 30_002)
//        .AddAzureTableGrainStorage("PubSubStore", options => options.ConfigureTableServiceClient(asConnString))
//        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(asConnString))
//        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(UnitGrain).Assembly).WithReferences())
//        .AddAzureBlobGrainStorageAsDefault(opt =>
//        {
//            opt.ConfigureBlobServiceClient(asConnString);
//        })
//        ;
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
            options.SiloName = "Processing";
        })
        .ConfigureEndpoints(siloPort: 11_113, gatewayPort: 30_002)
        .AddAzureTableGrainStorage("PubSubStore", options => options.ConfigureTableServiceClient(asConnString))
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(asConnString))
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(UnitGrain).Assembly).WithReferences())
        .AddAzureBlobGrainStorageAsDefault(opt =>
        {
            opt.ConfigureBlobServiceClient(asConnString);
        })
        ;
});

builder.Services.AddWebAppApplicationInsights("ProcessingSilo");

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Processing Silo V0.1 :)"));

app.Run();
