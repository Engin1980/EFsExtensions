using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Eng.EFsExtensions.Modules.FlightLogModule.RunModel
{
  public class LogViewModel : NotifyPropertyChanged
  {
    private class LogFlightComparer : IComparer<LogFlight>
    {
      public int Compare(LogFlight? x, LogFlight? y) => y!.StartUp.RealTime.CompareTo(x!.StartUp.RealTime);
    }
    private static LogFlightComparer logFlightComparer = new();

    public LogViewModel(LogFlightsManager flightsManager)
    {
      this.Flights = flightsManager.Flights.OrderByDescending(q => q.StartUp.RealTime).ToBindingList();
      this.RecentFlight = this.Flights.LastOrDefault();
      this.SelectedFlight = null;
      flightsManager.NewFlightLogged += f => this.AddFlight(f);
    }

    private void AddFlight(LogFlight flight)
    {
      int index = this.Flights.ToList().BinarySearch(flight, logFlightComparer);
      if (index < 0)
        index = ~index;

      this.Flights.Insert(index, flight);
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
