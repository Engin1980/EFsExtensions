using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models
{
  public partial class ActiveFlightViewModel
  {
    public record LandingAttemptData(double Bank, double Pitch, double IAS, double GS, double VS,
      TimeSpan MainGearTime,
      TimeSpan? AllGearTime,
      double MaxAccY,
      DateTime TouchDownDateTime, double TouchDownLatitude, double TouchDownLongitude,
      DateTime? RollOutEndDateTime, double? RollOutEndLatitude, double? RollOutEndLongitude)
    {
      public double? RollDistance => RollOutEndDateTime == null 
        ? null 
        : GpsCalculator.GetDistance(TouchDownLatitude, TouchDownLongitude, RollOutEndLatitude!.Value, RollOutEndLongitude!.Value);
      public TimeSpan? RollOutDuration => RollOutEndDateTime == null ? null : RollOutEndDateTime - TouchDownDateTime;
    }

    public record TakeOffAttemptData(double MaxBank, double MaxPitch, double IAS, double GS, double MaxVS,
      TimeSpan RollToFrontGearTime, TimeSpan RollToAllGearTime,
      double MaxAccY, DateTime RollStartDateTime, DateTime AirborneDateTime,
      double RollStartLatitude, double RollStartLongitude,
      double TakeOffLatitude, double TakeOffLongitude)
    {
      public double RollDistance => GpsCalculator.GetDistance(RollStartLatitude, RollStartLongitude, TakeOffLatitude, TakeOffLongitude);
      public TimeSpan RollDuration => AirborneDateTime - RollStartDateTime;
    }

    public record RunModelVatsimCache(FlightRules FlightType, string Callsign, string Aircraft, string? Registration,
      string DepartureICAO, string DestinationICAO, string AlternateICAO, string Route, int PlannedFlightLevel, int CruiseSpeed,
      DateTime PlannedDepartureTime, TimeSpan PlannedRouteTime, TimeSpan FuelDurationTime);
    public record RunModelSimBriefCache(string Callsign, string DepartureICAO, string DestinationICAO, string AlternateICAO,
      FlightRules FlightRules,
      DateTime OffBlockPlannedTime, DateTime TakeOffPlannedTime, DateTime LandingPlannedTime, DateTime OnBlockPlannedTime,
      int Altitude,
      int AirDistanceNM, int RouteDistanceNM,
      string AirplaneType, string AirplaneRegistration,
      int NumberOfPassengers, int PayLoad, int Cargo, int ZfwKg, int FuelKg, int EstimatedTakeOffFuelKg, int EstimatedLandingFuelKg);

    //TODO delete IAS & GPS & Date if not used
    public record RunModelTakeOffCache(DateTime Time, int FuelKg, double IAS, double Latitude, double Longitude);

    //TODO delete IAS & GPS & Date if not used
    public record RunModelStartUpCache(DateTime Time, int EmptyWeight, int PayloadAndCargoKg, int FuelKg, double Latitude, double Longitude)
    {
      public int ZFW => EmptyWeight + PayloadAndCargoKg;
    }
    public record RunModelShutDownCache(DateTime Time, int FuelKg, double Latitude, double Longitude);
    public record RunModelLandingCache(DateTime Time, int FuelKg, double IAS, double Latitude, double Longitude);
  }
}
