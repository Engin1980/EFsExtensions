using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling;
using ESystem.WPF.Windows;
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
  /// Interaction logic for LogFlight.xaml
  /// </summary>
  public partial class CtrLogFlight : UserControl
  {
    public CtrLogFlight()
    {
      InitializeComponent();
    }

    private void btnChangeRegistration_Click(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is not CtrLogFlightOverview.LogViewModel vm) return;
      if (vm.SelectedFlight == null) return;
      LoggedFlight lf = vm.SelectedFlight;

      var inputBox = new InputBox(
        "Enter new registration:",
        "Change registration...",
        lf.AircraftRegistration,
        validator: q => q.Trim().Length > 0,
        validationErrorMessage: "Registration must be non-empty.");
      if (inputBox.ShowDialog() == true)
      {
        lf.AircraftRegistration = inputBox.Input!.ToUpper();
        ProfileManager.UpdateFlight(lf);
        vm.SelectedFlight = null;
        vm.SelectedFlight = lf;

        int i = vm.Flights.IndexOf(lf);
        vm.Flights[i] = lf;
      }
    }
  }
}
