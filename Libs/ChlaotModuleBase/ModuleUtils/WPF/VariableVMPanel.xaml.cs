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
  /// Interaction logic for VariableVMPanel.xaml
  /// </summary>
  public partial class VariableVMPanel : UserControl
  {
    public static DependencyProperty ValueProperty = DependencyProperty.Register(
      nameof(Value), typeof(VariableVM), typeof(VariableVMPanel));

    public VariableVM Value
    {
      get => (VariableVM)GetValue(ValueProperty);
      set
      {
        SetValue(ValueProperty, value);
      }
    }

    public VariableVMPanel()
    {
      InitializeComponent();
    }
  }
}
