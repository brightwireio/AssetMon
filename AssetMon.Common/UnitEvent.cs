using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AssetMon.Common;

[Index(nameof(VbuNumber))]
public class UnitEvent
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public long VbuNumber { get; set; }
    public EventTypeEnum EventId { get; set; }
    public DateTime LocationDateTime { get; set; }
    public DateTime ServerDateTime { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Speed { get; set; }
    public int EngineHours { get; set; }
    public bool Ignition { get; set; }
}


public enum EventTypeEnum
{
    IgnitionOn,
    IgnitionOff,
    Location,
    HarshBreak,
    SpeedLimitViolation
}