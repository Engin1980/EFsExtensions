using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Globals;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Shared;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LoggedFlight
  {
    public int Version { get; set; } = 2;
    public string Callsign { get; set; } = string.Empty;
    public FlightRules FlightRules { get; set; } = FlightRules.Unknown;
    public string? DepartureICAO { get; set; } = string.Empty;
    public string? DestinationICAO { get; set; } = string.Empty;
    public string? AlternateICAO { get; set; } = string.Empty;
    public string LandedICAO { get; set; } = string.Empty;
    public int CruizeAltitude { get; set; }
    public double Distance { get; set; }
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

    public int NumberOfGoArounds { get; set; } = 0;
    public DateTime? StartUpScheduledDateTime { get; set; }
    public DateTime StartUpDateTime { get; set; }
    public DateTime? TakeOffScheduledDateTime { get; set; }
    public DateTime TakeOffDateTime => TakeOff.RunStartDateTime;
    public int? TakeOffScheduledFuelWeight { get; set; }
    public int TakeOffFuelWeight { get; set; }
    public DateTime? LandingScheduledDateTime { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public List<LoggedFlightTouchdown> Touchdowns { get; set; } = null!;
    public LoggedFlightTakeOff TakeOff { get; set; } = null!;
    public int? LandingScheduledFuelWeight { get; set; }
    public int LandingFuelWeight { get; set; }
    public DateTime? ShutDownScheduledDateTime { get; set; }
    public DateTime ShutDownDateTime { get; set; }
    public int ShutDownFuelWeight { get; set; }
    public GPS ShutDownLocation { get; set; }
    public GPS TakeOffLocation => TakeOff.RunStartLocation;
    public TimeSpan DepartureTaxiTime => this.TakeOffDateTime - this.StartUpDateTime;
    public int LandingIAS => Touchdowns.Last().IAS;
    public double LandingBank => Touchdowns.Last().Bank;
    public double LandingPitch => Touchdowns.Last().Pitch;
    public TimeSpan ArrivalTaxiTime => this.ShutDownDateTime - this.LandingDateTime;
    public TimeSpan TaxiTime => this.DepartureTaxiTime + this.ArrivalTaxiTime;
    public TimeSpan AirTime => this.LandingDateTime - this.TakeOffDateTime;
    public DateTime Time => Touchdowns.Last().TouchDownDateTime;
    public TimeSpan BlockTime => this.ShutDownDateTime - this.StartUpDateTime;
    public int TakeOffIAS => TakeOff.IAS;
    public GPS LandingLocation => Touchdowns.Last().TouchDownLocation;
    public DateTime LandingDateTime => Touchdowns.Last().TouchDownDateTime;
    public TimeSpan FlightDuration => LandingDateTime - TakeOffDateTime;
    public TimeSpan TotalDuration => ShutDownDateTime - StartUpDateTime;

    public void CheckValidity(out bool resaveNeeded)
    {
      AdjustVersion(out resaveNeeded);
      try
      {
        ValidateAllPropertiesByRead();
      }
      catch (Exception e)
      {
        throw new ApplicationException($"Flight '{Callsign}' is not valid: {e.Message}", e);
      }
    }

    private void ValidateAllPropertiesByRead()
    {
      var props = typeof(LoggedFlight).GetProperties(BindingFlags.Public | BindingFlags.Instance);
      foreach (PropertyInfo prop in props)
      {
        try
        {
          prop.GetValue(this);
        }
        catch (Exception e)
        {
          throw new ApplicationException($"Property '{prop.Name}' cannot be read: {e.Message}", e);
        }
      }
    }

    private void AdjustVersion(out bool resaveNeeded)
    {
      resaveNeeded = false;

      if (Version == 1)
      {
        this.LandedICAO = GetLandedAirportICAO();
        Version = 2;
        resaveNeeded = true;
      }
    }

    private string GetLandedAirportICAO()
    {
      GPS gps = this.Touchdowns.Last().TouchDownLocation;
      var ret = GlobalProvider.Instance.NavData.Airports.TryGetNearestAirport(gps);
      return ret?.ICAO ?? string.Empty;
    }
  }
}
