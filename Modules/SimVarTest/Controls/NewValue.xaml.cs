using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Eng.EFsExtensions.Modules.SimVarTestModule.Controls
{
  /// <summary>
  /// Interaction logic for NewValue.xaml
  /// </summary>
  public partial class NewValue : UserControl
  {
    public event Action<NewValue, double>? NewValueRequested;

    public NewValue()
    {
      InitializeComponent();
    }

    private void btnApply_Click(object sender, RoutedEventArgs e)
    {
      double d;
      try
      {
        d = double.Parse(txtValue.Text.Replace(",","."), CultureInfo.GetCultureInfo("en-US"));
      }
      catch (Exception)
      {
        return;
      }
      NewValueRequested?.Invoke(this, d);
    }
  }
}
