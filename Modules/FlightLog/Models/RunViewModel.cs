using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.SimBriefModel;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models
{
  public class RunViewModel : NotifyPropertyChanged
  {
    public record LandingAttemptData(double Bank, double Pitch, double IAS, double VS, double MainGearTime, double AllGearTime,
      double MaxAccY, DateTime DateTime,
      double Latitude, double Longitude);

    public record TakeOffAttemptData(double MaxBank, double MaxPitch, double IAS, double MaxVS,
      TimeSpan RollToFrontGearTime, TimeSpan RollToAllGearTime,
      double MaxAccY, DateTime RollStartDateTime,
      double RollStartLatitude, double RollStartLongitude,
      double TakeOffLatitude, double TakeOffLongitude)
    {
      public double RollDistance => GpsCalculator.GetDistance(RollStartLatitude, RollStartLongitude, TakeOffLatitude, TakeOffLongitude);
    }

    public record RunModelVatsimCache(string FlightRules, string Callsign, string Aircraft, string? Registration,
      string DepartureICAO, string DestinationICAO, string AlternateICAO, string Route, int PlannedFlightLevel, int CruiseSpeed,
      DateTime PlannedDepartureTime, TimeSpan PlannedRouteTime, TimeSpan FuelDurationTime);
    public record RunModelSimBriefCache(string DepartureICAO, string DestinationICAO, string AlternateICAO,
      DateTime OffBlockPlannedTime, DateTime TakeOffPlannedTime, DateTime LandingPlannedTime, DateTime OnBlockPlannedTime,
      int Altitude,
      int AirDistanceNM, int RouteDistanceNM,
      string AirplaneType, string AirplaneRegistration,
      int NumberOfPassengers, int PayLoad, int Cargo, int ZfwKg, int FuelKg, int EstimatedTakeOffFuelKg, int EstimatedLandingFuelKg);
    public record RunModelTakeOffCache(DateTime Time, int FuelKg, double IAS, double Latitude, double Longitude);
    public record RunModelStartUpCache(DateTime Time, int EmptyWeight, int PayloadAndCargoKg, int FuelKg, double Latitude, double Longitude)
    {
      public int ZFW => EmptyWeight + PayloadAndCargoKg;
    }
    public record RunModelShutDownCache(DateTime Time, int FuelKg, double Latitude, double Longitude);
    public record RunModelLandingCache(DateTime Time, int FuelKg, double IAS, double Latitude, double Longitude);

    public enum RunModelState
    {
      WaitingForStartupForTheFirstTime,
      StartedWaitingForTakeOff,
      InFlightWaitingForLanding,
      LandedWaitingForShutdown,
      WaitingForStartupAfterShutdown
    }

    public int MaxAchievedAltitude
    {
      get => GetProperty<int>(nameof(MaxAchievedAltitude))!;
      set => UpdateProperty(nameof(MaxAchievedAltitude), value);
    }

    public RunModelState State
    {
      get { return GetProperty<RunModelState>(nameof(State))!; }
      set { UpdateProperty(nameof(State), value); }
    }

    public RunModelVatsimCache? VatsimCache
    {
      get => GetProperty<RunModelVatsimCache?>(nameof(VatsimCache))!;
      set => UpdateProperty(nameof(VatsimCache), value);
    }

    public RunModelSimBriefCache? SimBriefCache
    {
      get => GetProperty<RunModelSimBriefCache?>(nameof(SimBriefCache))!;
      set => UpdateProperty(nameof(SimBriefCache), value);
    }

    public RunModelTakeOffCache? TakeOffCache
    {
      get { return GetProperty<RunModelTakeOffCache?>(nameof(TakeOffCache))!; }
      set { UpdateProperty(nameof(TakeOffCache), value); }
    }

    public RunModelStartUpCache? StartUpCache
    {
      get { return GetProperty<RunModelStartUpCache?>(nameof(StartUpCache))!; }
      set { UpdateProperty(nameof(StartUpCache), value); }
    }

    public RunModelLandingCache? LandingCache
    {
      get { return GetProperty<RunModelLandingCache?>(nameof(LandingCache))!; }
      set { UpdateProperty(nameof(LandingCache), value); }
    }

    public RunModelShutDownCache? ShutDownCache
    {
      get { return GetProperty<RunModelShutDownCache?>(nameof(ShutDownCache))!; }
      set { UpdateProperty(nameof(ShutDownCache), value); }
    }

    public BindingList<LandingAttemptData> LandingAttempts
    {
      get { return GetProperty<BindingList<LandingAttemptData>?>(nameof(LandingAttempts))!; }
      set { UpdateProperty(nameof(LandingAttempts), value); }
    }

    public TakeOffAttemptData? TakeOffAttempt
    {
      get => base.GetProperty<TakeOffAttemptData?>(nameof(TakeOffAttempt))!;
      set => base.UpdateProperty(nameof(TakeOffAttempt), value);
    }

    public InitContext.Profile Profile
    {
      get => GetProperty<InitContext.Profile>(nameof(Profile))!;
      set => UpdateProperty(nameof(Profile), value);
    }

    public BindingList<LogFlight> LoggedFlights
    {
      get => GetProperty<BindingList<LogFlight>>(nameof(LoggedFlights))!;
      set => UpdateProperty(nameof(LoggedFlights), value);
    }

    public LogFlight? LastLoggedFlight
    {
      get => GetProperty<LogFlight?>(nameof(LastLoggedFlight))!;
      set => UpdateProperty(nameof(LastLoggedFlight), value);
    }

    public RunViewModel()
    {
      State = RunModelState.WaitingForStartupForTheFirstTime;
      LandingAttempts = new();
    }

    internal void Clear()
    {
      VatsimCache = null;
      SimBriefCache = null;
      StartUpCache = null;
      LandingCache = null;
      ShutDownCache = null;
      TakeOffCache = null;
      State = RunModelState.WaitingForStartupForTheFirstTime;
    }
  }
}
