using Eng.EFsExtensions.Libs.AirportsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public enum DivertReason
  {
    NotDiverted,
    Weather,
    Medical,
    Mechanical,
    Other
  }
  internal record LogStartUp(DateTime? ScheduledTime, DateTime RealTime, int RealFuelAmountKg, GPS Location);
  internal record LogTakeOff(DateTime? ScheduledTime, DateTime RealTime, int? ScheduledFuelAmountKg, int FuelAmountKg, GPS Location, int IAS);
  internal record LogLanding(DateTime? ScheduledTime, DateTime RealTime, int? ScheduledFuelAmountKg, int FuelAmountKg, GPS Location, int IAS, double Velocity, double Pitch);
  internal record LogShutDown(DateTime? ScheduledTime, DateTime RealTime, int FuelAmountKg, GPS Location);
  internal record LogFlight(string? DepartureICAO, string? DestinationICAO, string? AlternateICAO,
    double ZFW, int? PassengerCount, int? CargoWeight, int? FuelWeight,
    string? AircraftType, string? AircraftRegistration, string? AircraftModel,
    int cruizeAltitude, double airDistance, double? flightDistance,
    LogStartUp StartUp, LogTakeOff TakeOff, LogLanding Landing, LogShutDown ShutDown,
    DivertReason? DivertReason);
}
