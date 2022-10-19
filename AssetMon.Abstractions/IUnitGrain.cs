using AssetMon.Common;
using Orleans;

namespace AssetMon.Abstractions;

public interface IUnitGrain : IGrainWithIntegerKey
{
    public Task ProcessEvent(UnitEvent @event);

    public Task<UnitState> GetState();
}


public class UnitState
{
    public UnitState()
    {
        Events = new List<UnitEvent>();
    }
    public TripStateEnum TripState { get; set; }
    public DateTime? TripStartDateTime { get; set; }
    public int NumHarschBreaks { get; set; }
    public int NumViolations { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<UnitEvent> Events { get; set; }
}

public enum TripStateEnum
{
    Inactive,
    Active
}