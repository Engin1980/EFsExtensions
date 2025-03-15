using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EfsExtensions.Modules.FlightLogModule
{
  internal class RunModel : NotifyPropertyChanged
  {
    public record RunModelTakeOffCache(DateTime Time, double Latittude, double Longitude);
    public record RunModelStartUpCache(DateTime Time);
    public record RunModelShutDownCache(DateTime Time);
    public record RunModelLandingCache(DateTime Time, double Latitude, double Longitude);

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

    public RunModel()
    {
      State = RunModelState.WaitingForStartup;
    }
  }
}
