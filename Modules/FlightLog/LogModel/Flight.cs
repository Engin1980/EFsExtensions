using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FlightLogModule.LogModel
{
  public enum DivertReason
  {
    NotDiverted,
    Weather,
    Medical,
    Mechanical,
    Other
  }
  internal record GPS(double Latitude, double Longitude);
  internal record StartUp(DateTime Time, double FuelAmountKg);
  internal record TakeOff(DateTime Time, double FuelAmountKg);
  internal record Landing(DateTime Time, double FuelAmountKg, double LandingIAS, double LandingG, double LandingPitch, double LandingGPS);
  internal record ShutDown(DateTime Time, double FuelAmountKg);
  internal record Flight(string? DepartureICAO, string? DestinationICAO, 
    DateTime ScheduledOffBlockTime, DateTime ScheduledOnBlockTime, int? PassengerCount,
    string? AircraftType, string? AircraftRegistration, string AircraftModel,
    StartUp StartUp, TakeOff TakeOff, Landing Landing, ShutDown ShutDown, 
    DivertReason? DivertReason);
}
