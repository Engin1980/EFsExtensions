using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling;
using ESystem.Miscelaneous;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  /// <summary>
  /// Interaction logic for LogFlightOverview.xaml
  /// </summary>
  public partial class CtrLogFlightOverview : UserControl
  {
    internal class LogViewModel : NotifyPropertyChanged
    {
      public List<LoggedFlight> Flights
      {
        get => base.GetProperty<List<LoggedFlight>>(nameof(Flights))!;
        set => base.UpdateProperty(nameof(Flights), value);
      }

      public LoggedFlight? SelectedFlight
      {
        get => base.GetProperty<LoggedFlight?>(nameof(SelectedFlight))!;
        set => base.UpdateProperty(nameof(SelectedFlight), value);
      }

      public StatsData Stats
      {
        get => base.GetProperty<StatsData>(nameof(Stats))!;
        set => base.UpdateProperty(nameof(Stats), value);
      }
    }

    private static readonly DependencyProperty VMProperty =
      DependencyProperty.Register(nameof(VM), typeof(LogViewModel), typeof(CtrLogFlightOverview));
    internal LogViewModel VM
    {
      get => (LogViewModel)GetValue(VMProperty);
      private set => SetValue(VMProperty, value);
    }

    private static readonly DependencyProperty FlightsProperty =
      DependencyProperty.Register(nameof(Flights), typeof(List<LoggedFlight>), typeof(CtrLogFlightOverview), new PropertyMetadata(null, OnFlighsPropertyChanged));

    private static void OnFlighsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.Property.Name == nameof(Flights) && d is CtrLogFlightOverview control)
      {
        var flights = (List<LoggedFlight>)e.NewValue;
        flights = flights.OrderByDescending(q => q.StartUpDateTime).ToList();
        control.VM.Flights = flights;
        control.VM.SelectedFlight = flights.FirstOrDefault();
        control.VM.Stats = ProfileManager.GetFlightsStatsData(flights);
      }
    }

    public List<LoggedFlight> Flights
    {
      get => (List<LoggedFlight>)GetValue(FlightsProperty);
      set => SetValue(FlightsProperty, value);
    }

    public CtrLogFlightOverview()
    {
      InitializeComponent();
      this.VM = new LogViewModel();
    }
  }
}
