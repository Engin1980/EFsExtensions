using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LogFlight
  {
    public string Callsign { get; set; } = string.Empty;
    public FlightRules FlightRules { get; set; } = FlightRules.Unknown;
    public string? DepartureICAO { get; set; } = string.Empty;
    public string? DestinationICAO { get; set; } = string.Empty;
    public string? AlternateICAO { get; set; } = string.Empty;
    public int CruizeAltitude { get; set; }
    public double AirDistance { get; set; }
    public double? FlightDistance { get; set; }
    public DivertReason? DivertReason { get; set; } = null!;
    public GPS StartupLocation { get; set; }
    public double ZFW { get; set; }
    public int? PassengerCount { get; set; }
    public int? CargoWeight { get; set; }
    public int StartUpFuelWeight { get; set; }
    public int? ScheduledTakeOffFuelWeight { get; set; }
    public string? AircraftType { get; set; }
    public string? AircraftRegistration { get; set; }
    public string? AircraftModel { get; set; }
    public DateTime? StartUpScheduledDateTime { get; set; }
    public DateTime StartUpDateTime { get; set; }
    public DateTime? TakeOffScheduledDateTime { get; set; }
    public DateTime TakeOffDateTime { get; set; }
    public int TakeOffIAS { get; set; }
    public int? TakeOffScheduledFuelWeight { get; set; }
    public int TakeOffFuelWeight { get; set; }
    public GPS TakeOffLocation { get; set; }
    public DateTime? LandingScheduledDateTime { get; set; }
    public DateTime LandingDateTime => Touchdowns.Last().DateTime;
    public DateTime? ScheduledTime { get; set; }
    public List<LogTouchdown> Touchdowns { get; set; } = null!;
    public DateTime Time => Touchdowns.Last().DateTime;
    public int? LandingScheduledFuelWeight { get; set; }
    public int LandingFuelWeight { get; set; }
    public GPS LandingLocation => Touchdowns.Last().Location;
    public int LandingIAS => Touchdowns.Last().IAS;
    public double LandingBank => Touchdowns.Last().Bank;
    public double LandingPitch => Touchdowns.Last().Pitch;
    public DateTime? ShutDownScheduledDateTime { get; set; }
    public DateTime ShutDownDateTime { get; set; }
    public int ShutDownFuelWeight { get; set; }
    public GPS ShutDownLocation { get; set; }
    public TimeSpan DepartureTaxiTime => this.TakeOffDateTime - this.StartUpDateTime;
    public TimeSpan ArrivalTaxiTime => this.ShutDownDateTime - this.LandingDateTime;
    public TimeSpan TaxiTime => this.DepartureTaxiTime + this.ArrivalTaxiTime;
    public TimeSpan AirTime => this.LandingDateTime - this.TakeOffDateTime;
    public TimeSpan BlockTime => this.ShutDownDateTime - this.StartUpDateTime;
  }

}
