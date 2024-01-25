using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
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

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF
{
  /// <summary>
  /// Interaction logic for PropertiesDataGrid.xaml
  /// </summary>
  public partial class PropertiesDataGrid : UserControl
  {
    public PropertyVMS Properties { get => (PropertyVMS)GetValue(PropertiesProperty); set => SetValue(PropertiesProperty, value); }

    public static DependencyProperty PropertiesProperty = DependencyProperty.Register(
      nameof(Properties), typeof(PropertyVMS), typeof(PropertiesDataGrid));

    public PropertiesDataGrid()
    {
      InitializeComponent();
    }
  }
}
