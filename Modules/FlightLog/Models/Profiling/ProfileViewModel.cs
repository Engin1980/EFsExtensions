using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling
{
  public class ProfileViewModel : NotifyPropertyChanged
  {
    public List<Profile> Profiles
    {
      get => GetProperty<List<Profile>>(nameof(Profiles))!;
      set => UpdateProperty(nameof(Profiles), value);
    }

    public Profile? SelectedProfile
    {
      get => GetProperty<Profile?>(nameof(SelectedProfile))!;
      set => UpdateProperty(nameof(SelectedProfile), value);
    }


    public List<LoggedFlight> SelectedProfileFlights
    {
      get => GetProperty<List<LoggedFlight>>(nameof(SelectedProfileFlights))!;
      set => UpdateProperty(nameof(SelectedProfileFlights), value);
    }

    public StatsData SelectedProfileStats
    {
      get => GetProperty<StatsData>(nameof(SelectedProfileStats))!;
      set => UpdateProperty(nameof(SelectedProfileStats), value);
    }
  }
}
