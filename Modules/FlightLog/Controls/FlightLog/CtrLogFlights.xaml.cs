using Eng.EFsExtensions.Modules.FlightLogModule.Controls.Shared;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem.Miscelaneous;
using System;
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
  /// Interaction logic for CtrLogFlights.xaml
  /// </summary>
  public partial class CtrLogFlights : FlightsBasedUserControl
  {
    public class CtrLogFlightsViewModel : NotifyPropertyChanged
    {
      public List<LoggedFlight> Flights
      {
        get => base.GetProperty<List<LoggedFlight>?>(nameof(Flights)) ?? [];
        set => base.UpdateProperty<List<LoggedFlight>>(nameof(Flights), value);
      }

      public List<LoggedFlight> FilteredFlights
      {
        get => base.GetProperty<List<LoggedFlight>?>(nameof(FilteredFlights)) ?? [];
        set => base.UpdateProperty<List<LoggedFlight>>(nameof(FilteredFlights), value);
      }

      public LoggedFlight? SelectedFlight
      {
        get => base.GetProperty<LoggedFlight?>(nameof(SelectedFlight));
        set => base.UpdateProperty<LoggedFlight?>(nameof(SelectedFlight), value);
      }
    }
    private readonly CtrLogFlightsViewModel vm;
    public CtrLogFlights()
    {
      InitializeComponent();
      this.pnlMain.DataContext = vm = new() { Flights = this.Flights };
      this.FlightsChanged += () =>
      {
        this.vm.Flights = this.Flights;
        ctrFilter.SetUpFilter(this.Flights);
        UpdateFilteredFlights();
      };
      this.ctrFilter.FilterChanged += UpdateFilteredFlights;
      UpdateFilteredFlights();
    }

    private void UpdateFilteredFlights()
    {
      var flights = ctrFilter.ApplyFilter(this.vm.Flights);
      this.vm.FilteredFlights = flights;
    }

    private void btnCloseSelectedFlight_Click(object sender, RoutedEventArgs e)
    {
      this.vm.SelectedFlight = null;
    }
  }
}
