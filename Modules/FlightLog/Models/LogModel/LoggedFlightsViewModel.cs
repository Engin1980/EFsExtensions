//using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
//using Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling;
//using ESystem;
//using ESystem.Miscelaneous;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Controls.Primitives;

//namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel
//{
//  public class LoggedFlightsViewModel : NotifyPropertyChanged
//  {
//    #region Static - comparing
//    private class LogFlightComparer : IComparer<LoggedFlight>
//    {
//      public int Compare(LoggedFlight? x, LoggedFlight? y) => y!.StartUpDateTime.CompareTo(x!.StartUpDateTime);
//    }
//    private readonly static LogFlightComparer logFlightComparer = new();
//    #endregion

//    //public LoggedFlightsViewModel(ProfileManager flightsManager)
//    //{
//    //  this.flightsManager = flightsManager;
//    //  flightsManager.NewFlightLogged += f => AddFlight(f);
//    //  flightsManager.StatsUpdated += FlightsManager_StatsUpdated;

//    //  Flights = flightsManager.Flights.OrderByDescending(q => q.StartUpDateTime).ToBindingList();
//    //  RecentFlight = Flights.LastOrDefault();
//    //  SelectedFlight = null;

//    //  Stats = flightsManager.StatsData;
//    //}

//    public BindingList<LoggedFlight> Flights
//    {
//      get => GetProperty<BindingList<LoggedFlight>>(nameof(Flights))!;
//      set => UpdateProperty(nameof(Flights), value);
//    }

//    public LoggedFlight? RecentFlight
//    {
//      get => GetProperty<LoggedFlight?>(nameof(RecentFlight))!;
//      private set => UpdateProperty(nameof(RecentFlight), value);
//    }

//    public LoggedFlight? SelectedFlight
//    {
//      get => GetProperty<LoggedFlight?>(nameof(SelectedFlight))!;
//      set => UpdateProperty(nameof(SelectedFlight), value);
//    }

//    public StatsData Stats
//    {
//      get => GetProperty<StatsData>(nameof(Stats))!;
//      set => UpdateProperty(nameof(Stats), value);
//    }

//    //private void FlightsManager_StatsUpdated()
//    //{
//    //  Stats = flightsManager.StatsData;
//    //}

//    //private void AddFlight(LoggedFlight flight)
//    //{
//    //  int index = Flights.ToList().BinarySearch(flight, logFlightComparer);
//    //  if (index < 0)
//    //    index = ~index;

//    //  Flights.Insert(index, flight);
//    //  RecentFlight = flight;
//    //}
//  }
//}
