using Eng.EFsExtensions.Modules.FlightLogModule.SimBriefModel;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  internal class RunViewModel : NotifyPropertyChanged
  {
    public record RunModelVatsimCache(string FlightRules, string Callsign, string Aircraft, string DepartureICAO, string DestinationICAO, string AlternateICAO, string Route, string PlannedFlightLevel, DateTime PlannedDepartureTime, TimeSpan PlannedRouteTime);
    public record RunModelSimDataCache(string DepartureICAO, string DestinationICAO, string AlternateICAO,
      DateTime OffBlockPlannedTime, DateTime OnBlockPlannedTime,
      int AirDistanceNM, int RouteDistanceNM,
      string AirplaneType, string AirplaneRegistration,
      int NumberOfPassengers, int PayLoad, int Cargo, int ZFW, int EstimatedTOW, int EstimatedLW);
    public record RunModelTakeOffCache(DateTime Time, double TotalFuel, double IAS, double Latitude, double Longitude);
    public record RunModelStartUpCache(DateTime Time, double TotalFuel, double Latitude, double Longitude);
    public record RunModelShutDownCache(DateTime Time, double TotalFuel, double Latitude, double Longitude);
    public record RunModelLandingCache(DateTime Time, double TotalFuel, double IAS,
      double TouchdownBankDegrees, double TouchdownLatitude, double TouchdownLongitude, double TouchdownVelocity, double TouchdownPitchDegrees,
      double Latitude, double Longitude);

    // TOTAL WEIGHT
    // TOTAL VELOCITY




    public enum RunModelState
    {
      WaitingForStartup,
      StartedWaitingForTakeOff,
      InFlightWaitingForLanding,
      LandedWaitingForShutdown
    }


    public RunModelState State
    {
      get { return base.GetProperty<RunModelState>(nameof(State))!; }
      set { base.UpdateProperty(nameof(State), value); }
    }


    public RunModelVatsimCache? VatsimCache
    {
      get => base.GetProperty<RunModelVatsimCache?>(nameof(VatsimCache))!;
      set => base.UpdateProperty(nameof(VatsimCache), value);
    }

    public RunModelSimDataCache? SimDataCache
    {
      get => base.GetProperty<RunModelSimDataCache?>(nameof(SimDataCache))!;
      set => base.UpdateProperty(nameof(SimDataCache), value);
    }

    public RunModelTakeOffCache? TakeOffCache
    {
      get { return base.GetProperty<RunModelTakeOffCache?>(nameof(TakeOffCache))!; }
      set { base.UpdateProperty(nameof(TakeOffCache), value); }
    }

    public RunModelStartUpCache? StartUpCache
    {
      get { return base.GetProperty<RunModelStartUpCache?>(nameof(StartUpCache))!; }
      set { base.UpdateProperty(nameof(StartUpCache), value); }
    }

    public RunModelLandingCache? LandingCache
    {
      get { return base.GetProperty<RunModelLandingCache?>(nameof(LandingCache))!; }
      set { base.UpdateProperty(nameof(LandingCache), value); }
    }

    public RunModelShutDownCache? ShutDownCache
    {
      get { return base.GetProperty<RunModelShutDownCache?>(nameof(ShutDownCache))!; }
      set { base.UpdateProperty(nameof(ShutDownCache), value); }
    }

    public InitContext.Profile Profile
    {
      get => base.GetProperty<InitContext.Profile>(nameof(Profile))!;
      set => base.UpdateProperty(nameof(Profile), value);
    }

    public RunViewModel()
    {
      State = RunModelState.WaitingForStartup;
    }
  }
}
