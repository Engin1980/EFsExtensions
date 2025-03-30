using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using ESystem;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models
{
  public class LogViewModel : NotifyPropertyChanged
  {
    #region Static - comparing
    private class LogFlightComparer : IComparer<LogFlight>
    {
      public int Compare(LogFlight? x, LogFlight? y) => y!.StartUp.Time.CompareTo(x!.StartUp.Time);
    }
    private static LogFlightComparer logFlightComparer = new();
    #endregion

    private readonly LogFlightsManager flightsManager;

    public LogViewModel(LogFlightsManager flightsManager)
    {
      this.flightsManager = flightsManager;
      flightsManager.NewFlightLogged += f => AddFlight(f);
      flightsManager.StatsUpdated += FlightsManager_StatsUpdated;

      Flights = flightsManager.Flights.OrderByDescending(q => q.StartUp.Time).ToBindingList();
      RecentFlight = Flights.LastOrDefault();
      SelectedFlight = null;

      Stats = flightsManager.StatsData;
    }    

    public BindingList<LogFlight> Flights
    {
      get => GetProperty<BindingList<LogFlight>>(nameof(Flights))!;
      set => UpdateProperty(nameof(Flights), value);
    }

    public LogFlight? RecentFlight
    {
      get => GetProperty<LogFlight?>(nameof(RecentFlight))!;
      set => UpdateProperty(nameof(RecentFlight), value);
    }

    public LogFlight? SelectedFlight
    {
      get => GetProperty<LogFlight?>(nameof(SelectedFlight))!;
      set => UpdateProperty(nameof(SelectedFlight), value);
    }

    public StatsData Stats
    {
      get => GetProperty<StatsData>(nameof(Stats))!;
      set => UpdateProperty(nameof(Stats), value);
    }

    private void FlightsManager_StatsUpdated()
    {
      Stats = flightsManager.StatsData;
    }
    
    private void AddFlight(LogFlight flight)
    {
      int index = Flights.ToList().BinarySearch(flight, logFlightComparer);
      if (index < 0)
        index = ~index;

      Flights.Insert(index, flight);
    }
  }
}
