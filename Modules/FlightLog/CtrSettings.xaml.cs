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
using System.Windows.Shapes;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  /// <summary>
  /// Interaction logic for CtrSettings.xaml
  /// </summary>
  public partial class CtrSettings : Window
  {
    private readonly Settings settings;
    public CtrSettings()
    {
      InitializeComponent();
      this.DataContext = this.settings = new Settings();
    }

    public CtrSettings(Settings settings)
    {
      InitializeComponent();
      this.DataContext = this.settings = settings;
    }

    private void btnBrowse_Click(object sender, RoutedEventArgs e)
    {
      //TODO implement
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      this.settings.Save();
    }
  }
}
