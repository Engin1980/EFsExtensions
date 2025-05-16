using Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.LogFlightOverview;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
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
  /// Interaction logic for CtrlLogDetailedStats.xaml
  /// </summary>
  public partial class CtrlLogDetailedStats : UserControl
  {
    public class ViewModel : NotifyPropertyChanged
    {
      public List<LoggedFlight> Flights
      {
        get => base.GetProperty<List<LoggedFlight>>(nameof(Flights))!;
        set => base.UpdateProperty(nameof(Flights), value);
      }

      public List<LoggedFlight> VisibleFlights
      {
        get => base.GetProperty<List<LoggedFlight>>(nameof(VisibleFlights))!;
        set => base.UpdateProperty(nameof(VisibleFlights), value);
      }

      public List<string> AirplaneRegistrations
      {
        get => base.GetProperty<List<string>>(nameof(AirplaneRegistrations))!;
        set => base.UpdateProperty(nameof(AirplaneRegistrations), value);
      }

      public string SelectedAirplaneRegistration
      {
        get => base.GetProperty<string>(nameof(SelectedAirplaneRegistration))!;
        set
        {
          base.UpdateProperty(nameof(SelectedAirplaneRegistration), value ?? string.Empty);
          this.UpdateVisibleFlights();
        }
      }

      public List<string> AirplaneTypes
      {
        get => base.GetProperty<List<string>>(nameof(AirplaneTypes))!;
        set => base.UpdateProperty(nameof(AirplaneTypes), value);
      }

      public string SelectedAirplaneType
      {
        get => base.GetProperty<string>(nameof(SelectedAirplaneType))!;
        set
        {
          base.UpdateProperty(nameof(SelectedAirplaneType), value ?? string.Empty);
          this.UpdateVisibleFlights();
        }
      }

      public List<string> CallsignFilter
      {
        get => base.GetProperty<List<string>>(nameof(CallsignFilter))!;
        set
        {
          base.UpdateProperty(nameof(CallsignFilter), value);
          this.UpdateVisibleFlights();
        }
      }

      public string SelectedCallsignFilter
      {
        get => base.GetProperty<string>(nameof(SelectedCallsignFilter))!;
        set => base.UpdateProperty(nameof(SelectedCallsignFilter), value ?? string.Empty);
      }


      internal void Reset(List<LoggedFlight>? flights)
      {
        this.Flights = flights ?? new();
        this.VisibleFlights = this.Flights;
        this.AirplaneRegistrations = this.Flights
          .Select(x => x.AircraftRegistration)
          .Distinct()
          .Where(q => q is not null)
          .Cast<string>()
          .ToList();
        this.AirplaneRegistrations.Insert(0, "");
        this.AirplaneTypes = this.Flights
          .Select(x => x.AircraftType)
          .Distinct()
          .Where(q => q is not null)
          .Cast<string>()
          .ToList();
        this.AirplaneTypes.Insert(0, "");

        this.SelectedAirplaneRegistration = "";
        this.SelectedAirplaneType = "";
        this.SelectedCallsignFilter = "";
      }

      private void UpdateVisibleFlights()
      {
        IEnumerable<LoggedFlight> tmp = this.Flights;

        if (this.SelectedAirplaneRegistration?.Length > 0)
          tmp = tmp.Where(q => q.AircraftRegistration == this.SelectedAirplaneRegistration);

        if (this.SelectedAirplaneType?.Length > 0)
          tmp = tmp.Where(q => q.AircraftType == this.SelectedAirplaneType);

        if (this.SelectedCallsignFilter?.Length > 0)
          tmp = tmp.Where(q => q.Callsign.Contains(this.SelectedCallsignFilter));

        this.VisibleFlights = tmp.ToList();
      }

      public ViewModel(DataGrid grd)
      {
        this.Reset(new());
        this.SelectedCallsignFilter = "";
        this.SelectedAirplaneRegistration = "";
        this.SelectedAirplaneType = "";
      }
    }

    private static readonly DependencyProperty FlightsProperty = DependencyProperty.Register(nameof(Flights), typeof(List<LoggedFlight>), typeof(CtrlLogDetailedStats),
      new PropertyMetadata() { PropertyChangedCallback = FlightsPropertyChanged });

    private static void FlightsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is CtrlLogDetailedStats ctrl)
        ctrl.FlightsChanged((List<LoggedFlight>?)e.NewValue);
    }

    private void FlightsChanged(List<LoggedFlight>? newValue)
    {
      this.VM.Reset(newValue);
    }

    public List<LoggedFlight> Flights
    {
      get => (List<LoggedFlight>)GetValue(FlightsProperty);
      set => SetValue(FlightsProperty, value);
    }

    public ViewModel VM { get; init; }

    public CtrlLogDetailedStats()
    {
      InitializeComponent();
      this.pnlContent.DataContext = this.VM = new ViewModel(this.grdVisibleFlights);
      this.VM.Reset(new List<LoggedFlight>());
    }

    private void btnColumns_Click(object sender, RoutedEventArgs e)
    {
      Dictionary<string, bool> columnsVisibility = grdVisibleFlights
        .Columns
        .ToDictionary(q => q.Header.ToString() ?? "", q => q.Visibility == Visibility.Visible);

      FrmVisibleColumns frm = new FrmVisibleColumns();
      frm.Init(columnsVisibility);
      frm.ShowDialog();
      columnsVisibility = frm.GetResultDictionary();
      foreach (var col in grdVisibleFlights.Columns)
      {
        if (columnsVisibility.TryGetValue(col.Header.ToString() ?? "", out bool isVisible))
          col.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
      }
    }
  }
}
