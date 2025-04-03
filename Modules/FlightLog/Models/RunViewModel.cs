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
  public partial class RunViewModel : NotifyPropertyChanged
  {
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
