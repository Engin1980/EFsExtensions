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
  /// Interaction logic for LogFlightToolTip.xaml
  /// </summary>
  public partial class CtrLogFlightToolTip : UserControl
  {
    public static DependencyProperty LogFlightProperty = DependencyProperty.Register(
      nameof(LogFlight),
      typeof(Eng.EFsExtensions.Modules.FlightLogModule.LogModel.LogFlight),
      typeof(CtrLogFlightToolTip),
      new PropertyMetadata(null));


    public Eng.EFsExtensions.Modules.FlightLogModule.LogModel.LogFlight? LogFlight
    {
      get => GetValue(LogFlightProperty) as Eng.EFsExtensions.Modules.FlightLogModule.LogModel.LogFlight;
      set => SetValue(LogFlightProperty, value);
    }
    public CtrLogFlightToolTip()
    {
      InitializeComponent();
    }
  }
}
