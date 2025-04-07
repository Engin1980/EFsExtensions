using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
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
    private Action onReadySet;
    private readonly Settings settings;

    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      set => base.UpdateProperty(nameof(IsReady), value);
    }

    public BindingList<Profile> Profiles
    {
      get => base.GetProperty<BindingList<Profile>>(nameof(Profiles))!;
      set => base.UpdateProperty(nameof(Profiles), value);
    }

    public Profile? SelectedProfile
    {
      get => base.GetProperty<Profile?>(nameof(SelectedProfile))!;
      set => base.UpdateProperty(nameof(SelectedProfile), value);
    }

    public List<LoggedFlight> LoggedFlights
    {
      get => base.GetProperty<List<LoggedFlight>>(nameof(LoggedFlights))!;
      set => base.UpdateProperty(nameof(LoggedFlights), value);
    }

    public List<Airport> Airports
    {
      get => this.settings.Airports;
    }

    public InitContext(Settings settings, Action onReadySet)
    {
      this.onReadySet = onReadySet;
      this.settings = settings;
      this.Profiles = new();
      IsReady = false;

      this.Profiles = ProfileManager.GetAvailableProfiles(settings.DataFolder).ToBindingList();
      this.SelectedProfile = Profiles.FirstOrDefault();

      this.PropertyChanged += OnPropertyChanged;
      ReloadFlightLog();


      // DEBUG STUFF, DELETE LATER
      //UpdateSimbriefAndVatsimIfRequired();
      //this.RunVM.StartUpCache = new RunViewModel.RunModelStartUpCache(DateTime.Now.AddMinutes(-70), 49000, 174 * 95, 5500, 32.6979, -16.7745);
      //this.RunVM.TakeOffCache = new RunViewModel.RunModelTakeOffCache(DateTime.Now.AddMinutes(-60), 5200, 137, -1, -1);
      //this.RunVM.LandingCache = new RunViewModel.RunModelLandingCache(DateTime.Now.AddMinutes(-10), 2100, 120, -1, -1);
      //this.RunVM.ShutDownCache = new RunViewModel.RunModelShutDownCache(DateTime.Now, 2000, 33.0734, -16.3498);
      //this.RunVM.LandingAttempts.Add(
      //  new RunViewModel.LandingAttemptData(0.2497133, 0.4941731, 150, 143, -304.1031372,
      //    TimeSpan.FromSeconds(0.4231), null,
      //    14.10721, DateTime.Now.AddSeconds(-1.242), 33.0769278, 16.3204778, null, null, null));
      //this.RunVM.LandingAttempts.Add(
      //  new RunViewModel.LandingAttemptData(0.1497133, 0.1941731, 140, 123, -104.1031372,
      //    TimeSpan.FromSeconds(0.0123), TimeSpan.FromSeconds(4.2312),
      //    4.10721, DateTime.Now, 33.0869278, 16.3504778, DateTime.Now.AddSeconds(38.123), 33.0598778, 16.3494139));
      //this.RunVM.TakeOffAttempt = new RunViewModel.TakeOffAttemptData(
      //  0.21243, 19.412, 154, 134, 3400.13213,
      //  new TimeSpan(0, 0, 0, 43, 121), new TimeSpan(0, 0, 0, 48, 313),
      //  13.1413,
      //  DateTime.Now.AddMinutes(-180), DateTime.Now.AddMinutes(-180).AddSeconds(49.112),
      //  32.7062, -16.7652,
      //   32.6934, -16.7784);

      ////var fl = GenerateLogFlight(this.RunVM);
      //fl.DepartureICAO = "CYVR";
      //fl.DestinationICAO = "CYYC";
      //fl.AlternateICAO = "CYEG";
      //ProfileManager.SaveFlight(new LoggedFlight()
      //{
      //  Touchdowns = new List<LoggedFlightTouchdown>()
      //  {
      //    new LoggedFlightTouchdown()
      //    {
      //      TouchDownDateTime = DateTime.Now,
      //      Bank = 0.1,
      //      Pitch = 0.2,
      //      IAS = 150
      //    }
      //  },
      //}, this.SelectedProfile);
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
