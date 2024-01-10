using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
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
  /// Interaction logic for MetaInfoPanel.xaml
  /// </summary>
  public partial class MetaInfoPanel : UserControl
  {
    public static DependencyProperty ValueProperty = DependencyProperty.Register(
      nameof(Value), typeof(MetaInfo), typeof(MetaInfoPanel));

    public MetaInfo Value
    {
      get => (MetaInfo)GetValue(ValueProperty); set
      {
        SetValue(ValueProperty, value);
        this.DataContext = value;
      }
    }
    public MetaInfoPanel()
    {
      InitializeComponent();
    }
  }
}
