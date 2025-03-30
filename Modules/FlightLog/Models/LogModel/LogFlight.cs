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
    public string? Callsign { get; set; }
    public string FlightRules { get; set; } = null!;
    public string? DepartureICAO { get; set; }
    public string? DestinationICAO { get; set; }
    public string? AlternateICAO { get; set; }
    public double ZFW { get; set; }
    public int? PassengerCount { get; set; }
    public int? CargoWeight { get; set; }
    public int? FuelWeight { get; set; }
    public string? AircraftType { get; set; }
    public string? AircraftRegistration { get; set; }
    public string? AircraftModel { get; set; }
    public int CruizeAltitude { get; set; }
    public double AirDistance { get; set; }
    public double? FlightDistance { get; set; }
    public TimeSpan? ScheduledFlightDuration => this.ShutDown.ScheduledTime - this.StartUp.ScheduledTime;
    public LogStartUp StartUp { get; set; } = null!;
    public LogTakeOff TakeOff { get; set; } = null!;
    public LogLanding Landing { get; set; } = null!;
    public LogShutDown ShutDown { get; set; } = null!;
    public DivertReason? DivertReason { get; set; } = null!;

    public TimeSpan DepartureTaxiTime => this.TakeOff.Time - this.StartUp.Time;
    public TimeSpan ArrivalTaxiTime => this.ShutDown.Time - this.Landing.Time;
    public TimeSpan TaxiTime => this.DepartureTaxiTime + this.ArrivalTaxiTime;
    public TimeSpan AirTime => this.Landing.Time - this.TakeOff.Time;
    public TimeSpan BlockTime => this.ShutDown.Time - this.StartUp.Time;

    public LogFlight()
    {
    }

    public LogFlight(string? departureICAO, string? destinationICAO, string? alternateICAO, double zFW, int? passengerCount, int? cargoWeight, int? fuelWeight, string? aircraftType, string? aircraftRegistration, string? aircraftModel, int cruizeAltitude, double airDistance, double? flightDistance, LogStartUp startUp, LogTakeOff takeOff, LogLanding landing, LogShutDown shutDown, DivertReason? divertReason)
    {
      DepartureICAO = departureICAO;
      DestinationICAO = destinationICAO;
      AlternateICAO = alternateICAO;
      ZFW = zFW;
      PassengerCount = passengerCount;
      CargoWeight = cargoWeight;
      FuelWeight = fuelWeight;
      AircraftType = aircraftType;
      AircraftRegistration = aircraftRegistration;
      AircraftModel = aircraftModel;
      CruizeAltitude = cruizeAltitude;
      AirDistance = airDistance;
      FlightDistance = flightDistance;
      StartUp = startUp;
      TakeOff = takeOff;
      Landing = landing;
      ShutDown = shutDown;
      DivertReason = divertReason;
    }
  }

}
