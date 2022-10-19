namespace AssetMon.Infrastructure.Entities;

public class Trip
{
    public long UnitId { get; set; }
    public Guid TripId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int NumHarschBreaks { get; set; }
    public int NumViolations { get; set; }
}
