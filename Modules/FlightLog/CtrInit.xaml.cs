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
using Eng.Chlaot.Modules.FlightLogModule;

namespace FlightLogModule
{
  /// <summary>
  /// Interaction logic for CtrInit.xaml
  /// </summary>
  public partial class CtrInit : UserControl
  {
    private readonly Context Context = null!;
    private bool isInitializing = false;

    public CtrInit()
    {
      isInitializing = true;
      InitializeComponent();
      isInitializing = false;
    }

    public CtrInit(Context context) : this()
    {
      this.Context = context;
      this.DataContext = context;
    }

    private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (tabMain.SelectedIndex == 1)
        lblTakeLong.Visibility = Visibility.Collapsed;
    }

    private void cmbProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (isInitializing) return;
      Context.IsReady = cmbProfile.SelectedIndex != 0;
    }
  }
}
