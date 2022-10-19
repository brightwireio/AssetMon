using AssetMon.Common;
using AssetMon.Infrastructure.Entities;

namespace AssetMon.Infrastructure.Repositories;

public interface IAssetMonRepository : IRepository
{
    IEnumerable<Trip> GetTripsForUnit(long unitId);

    void AddTrip(Trip trip);

    void AddUnitEvent(UnitEvent @event);


    IEnumerable<UnitEvent> GetUnitEvents();

    List<UnitEvent> GetEventsForTrip(Guid tripId);


    Task<Unit> GetUnit(long unitId);
}
