using Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.Stats.DescriptiveStats;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Printing;
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
  /// Interaction logic for CtrDescriptiveStats.xaml
  /// </summary>
  public partial class CtrDescriptiveStats : UserControl
  {
    
    public CtrDescriptiveStats()
    {
      InitializeComponent();
    }

    private void btnShowCharts_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      DescriptiveLogStatView stats = (DescriptiveLogStatView)btn.Tag;

      CtrLogStats.StatsData statsData = FindStatsData(btn);

      DescriptiveStatsHistogram frm = new DescriptiveStatsHistogram();
      frm.SetData(statsData, stats);
      frm.Show();
    }

    private CtrLogStats.StatsData FindStatsData(Button btn)
    {
      CtrLogStats.StatsData? ret = null;
      FrameworkElement? element = btn;

      while (element != null)
      {
        object dtc = element.DataContext;
        if (dtc is CtrLogStats.StatsData)
        {
          ret = (CtrLogStats.StatsData)dtc;
          break;
        }
        else
        {
          element = VisualTreeHelper.GetParent(element) as FrameworkElement;
        }
      }
      if (ret == null)
      {
        throw new InvalidOperationException("Cannot find StatsData in visual tree.");
      }
      return ret;
    }
  }
}
