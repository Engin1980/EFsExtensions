using Eng.EFsExtensions.Libs.AirportsLib;
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

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Controls
{
  /// <summary>
  /// Interaction logic for AirportsTreeView.xaml
  /// </summary>
  public partial class AirportsTreeView : UserControl
  {
    private static readonly DependencyProperty AirportsProperty = DependencyProperty.Register(
      nameof(Airports), typeof(List<Airport>), typeof(AirportsTreeView));

    public List<Airport> Airports
    {
      get => (List<Airport>)GetValue(AirportsProperty);
      set => SetValue(AirportsProperty, value);
    }

    public AirportsTreeView()
    {
      InitializeComponent();
    }
  }
}
