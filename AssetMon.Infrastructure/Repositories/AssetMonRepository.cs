using AssetMon.Abstractions;
using AssetMon.Common;
using AssetMon.Infrastructure.Entities;
using AssetMon.Infrastructure.Persistence;

namespace AssetMon.Infrastructure.Repositories;

public class AssetMonRepository : IAssetMonRepository
{
    private readonly AssetMonDbContext _dbContext;
    private readonly IBlobService _blobService;
    private readonly IClusterClient _clusterClient;

    public AssetMonRepository(AssetMonDbContext dbContext, IBlobService blobService, IClusterClient clusterClient)
    {
        _dbContext = dbContext;
        _blobService = blobService;
        _clusterClient = clusterClient;
    }

    public void AddTrip(Trip trip)
    {
        _dbContext.Trips.Add(trip);
    }

    public void AddUnitEvent(UnitEvent @event)
    {
        _dbContext.UnitEvents.Add(@event);
    }

    public List<UnitEvent> GetEventsForTrip(Guid tripId)
    {
        return JsonConvert.DeserializeObject<List<UnitEvent>>(_blobService.GetBlobString($"{tripId}.json", "tripdata"));
    }

    public IEnumerable<Trip> GetTripsForUnit(long unitId)
    {
        return _dbContext.Trips.Where(v => v.UnitId == unitId);
    }

    public async Task<Unit> GetUnit(long unitId)
    {
        var unitGrain = _clusterClient.GetGrain<IUnitGrain>(unitId);
        var unitState = await unitGrain.GetState();
        return new Unit(unitId, unitState.Latitude, unitState.Longitude, unitState.TripState.ToString());
    }

    public IEnumerable<UnitEvent> GetUnitEvents()
    {
        return _dbContext.UnitEvents;
    }

    public bool Save()
    {
        return _dbContext.SaveChanges() > 0;
    }


}
