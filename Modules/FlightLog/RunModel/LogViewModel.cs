using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.RunModel
{
  public class LogViewModel : NotifyPropertyChanged
  {
    public LogViewModel(LogFlightsManager flightsManager)
    {
      this.Flights = flightsManager.Flights.ToBindingList();
      this.RecentFlight = this.Flights.LastOrDefault();
      this.SelectedFlight = null;
      flightsManager.NewFlightLogged += f => this.Flights.Add(f);
    }

    public BindingList<LogFlight> Flights
    {
      get => base.GetProperty<BindingList<LogFlight>>(nameof(Flights))!;
      set => base.UpdateProperty(nameof(Flights), value);
    }


    public LogFlight? RecentFlight
    {
      get => base.GetProperty<LogFlight?>(nameof(RecentFlight))!;
      set => base.UpdateProperty(nameof(RecentFlight), value);
    }


    public LogFlight? SelectedFlight
    {
      get => base.GetProperty<LogFlight?>(nameof(SelectedFlight))!;
      set => base.UpdateProperty(nameof(SelectedFlight), value);
    }
  }
}
