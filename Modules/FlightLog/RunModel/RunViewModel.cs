using Eng.EFsExtensions.Modules.FlightLogModule.SimBriefModel;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class RunViewModel : NotifyPropertyChanged
  {
    public record RunModelVatsimCache(string FlightRules, string Callsign, string Aircraft, string? Registration,
      string DepartureICAO, string DestinationICAO, string AlternateICAO, string Route, int PlannedFlightLevel,
      DateTime PlannedDepartureTime, TimeSpan PlannedRouteTime, TimeSpan FuelDurationTime);
    public record RunModelSimBriefCache(string DepartureICAO, string DestinationICAO, string AlternateICAO,
      DateTime OffBlockPlannedTime, DateTime TakeOffPlannedTime, DateTime LandingPlannedTime, DateTime OnBlockPlannedTime,
      int Altitude,
      int AirDistanceNM, int RouteDistanceNM,
      string AirplaneType, string AirplaneRegistration,
      int NumberOfPassengers, int PayLoad, int Cargo, int ZfwKg, int FuelKg, int EstimatedTOW, int EstimatedLW);
    public record RunModelTakeOffCache(DateTime Time, int FuelKg, double IAS, double Latitude, double Longitude);
    public record RunModelStartUpCache(DateTime Time, int EmptyWeight, int PayloadAndCargoKg, int FuelKg, double Latitude, double Longitude)
    {
      public int ZFW => EmptyWeight + PayloadAndCargoKg;
    }
    public record RunModelShutDownCache(DateTime Time, int FuelKg, double Latitude, double Longitude);
    public record RunModelLandingCache(DateTime Time, int FuelKg, double IAS,
      double TouchdownBankDegrees, double TouchdownLatitude, double TouchdownLongitude, double TouchdownVelocity, double TouchdownPitchDegrees,
      double Latitude, double Longitude);

    public enum RunModelState
    {
      WaitingForStartup,
      StartedWaitingForTakeOff,
      InFlightWaitingForLanding,
      LandedWaitingForShutdown,
      AfterShutdown
    }


    public int MaxAchievedAltitude
    {
      get => base.GetProperty<int>(nameof(MaxAchievedAltitude))!;
      set => base.UpdateProperty(nameof(MaxAchievedAltitude), value);
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

    public RunModelSimBriefCache? SimBriefCache
    {
      get => base.GetProperty<RunModelSimBriefCache?>(nameof(SimBriefCache))!;
      set => base.UpdateProperty(nameof(SimBriefCache), value);
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

    internal void Clear()
    {
      this.VatsimCache = null;
      this.SimBriefCache = null;
      this.StartUpCache = null;
      this.LandingCache = null;
      this.ShutDownCache = null;
      this.TakeOffCache = null;
      this.State = RunModelState.WaitingForStartup;
    }
  }
}
