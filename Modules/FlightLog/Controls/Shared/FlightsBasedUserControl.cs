using Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.Shared;

public class FlightsBasedUserControl : UserControl
{
  protected event Action? FlightsChanged;

  private static readonly DependencyProperty FlightsProperty = DependencyProperty.Register(
    nameof(Flights),
    typeof(List<LoggedFlight>),
    typeof(FlightsBasedUserControl),
    new PropertyMetadata(null, OnFlighsPropertyChanged));

  public List<LoggedFlight> Flights
  {
    get => (List<LoggedFlight>)GetValue(FlightsProperty);
    set => SetValue(FlightsProperty, value);
  }

  private static void OnFlighsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (e.Property.Name == nameof(Flights) && d is FlightsBasedUserControl control)
    {
      var flights = (List<LoggedFlight>)e.NewValue;
      flights = flights.OrderByDescending(q => q.StartUpDateTime).ToList();
      //control.VM.Flights = flights;
      //control.VM.SelectedFlight = flights.FirstOrDefault();
      //control.VM.Fleet = ProfileManager.GetFleetStats(flights);
      control.FlightsChanged?.Invoke();
    }
  }
}
