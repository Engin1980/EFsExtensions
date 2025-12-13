using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
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

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.Stats
{
  /// <summary>
  /// Interaction logic for CtrlLogDetailedStats.xaml
  /// </summary>
  public partial class CtrDetailedStats : UserControl
  {
    public CtrDetailedStats()
    {
      InitializeComponent();
    }

    private void btnColumns_Click(object sender, RoutedEventArgs e)
    {
      Dictionary<string, bool> columnsVisibility = grdVisibleFlights
        .Columns
        .ToDictionary(q => q.Header.ToString() ?? "", q => q.Visibility == Visibility.Visible);

      FrmVisibleColumns frm = new FrmVisibleColumns();
      frm.Init(columnsVisibility);
      frm.ShowDialog();
      columnsVisibility = frm.GetResultDictionary();
      foreach (var col in grdVisibleFlights.Columns)
      {
        if (columnsVisibility.TryGetValue(col.Header.ToString() ?? "", out bool isVisible))
          col.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
      }
    }
  }
}
