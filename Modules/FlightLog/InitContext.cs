using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Globals;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.ActiveFlight.SimBriefModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class InitContext : NotifyPropertyChanged
  {
    private readonly Action<bool> onReadyChange;
    private readonly Settings settings;

    public bool IsActive
    {
      get => base.GetProperty<bool>(nameof(IsActive))!;
      set
      {
        base.UpdateProperty(nameof(IsActive), value);
        UpdateReadyFlag();
      }
    }

    public BindingList<Profile> Profiles
    {
      get => base.GetProperty<BindingList<Profile>>(nameof(Profiles))!;
      set => base.UpdateProperty(nameof(Profiles), value);
    }

    public Profile? SelectedProfile
    {
      get => base.GetProperty<Profile?>(nameof(SelectedProfile))!;
      set
      {
        base.UpdateProperty(nameof(SelectedProfile), value);
        UpdateReadyFlag();
      }
    }

    public List<LoggedFlight> LoggedFlights
    {
      get => base.GetProperty<List<LoggedFlight>>(nameof(LoggedFlights))!;
      set => base.UpdateProperty(nameof(LoggedFlights), value);
    }

    public List<Airport> Airports
    {
      get => base.GetProperty<List<Airport>>(nameof(Airports))!;
      set => base.UpdateProperty(nameof(Airports), value);
    }

    private void UpdateReadyFlag()
    {
      bool ready = this.SelectedProfile != null && this.IsActive;
      onReadyChange(ready);
    }

    public InitContext(Settings settings, Action<bool> onReadyChange)
    {
      this.onReadyChange = onReadyChange;
      this.settings = settings;
      this.Profiles = new();

      this.Airports = GlobalProvider.Instance.NavData.Airports.ToList();

      this.Profiles = ProfileManager.GetAvailableProfiles(settings.DataFolder).ToBindingList();
      this.SelectedProfile = Profiles.FirstOrDefault();

      this.PropertyChanged += OnPropertyChanged;
      ReloadFlightLog();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(SelectedProfile))
      {
        ReloadFlightLog();
      }
    }

    private void ReloadFlightLog()
    {
      if (SelectedProfile == null)
        this.LoggedFlights = new();
      else
      {
        this.LoggedFlights = ProfileManager.GetProfileFlights(SelectedProfile);
      }
    }
    internal void CreateProfile(string newProfileName)
    {
      Profile newProfile = ProfileManager.CreateProfile(settings.DataFolder, newProfileName);
      this.Profiles.Add(newProfile);
    }
  }
}
