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

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.Stats;

/// <summary>
/// Interaction logic for CtrGroupingStats.xaml
/// </summary>
public partial class CtrGroupingStats : UserControl
{
  public CtrGroupingStats()
  {
    InitializeComponent();
  }

  private void btnDetails_Click(object sender, RoutedEventArgs e)
  {
    Button btn = (Button)sender;
    var stat = (Models.LogModel.GroupingLogStatView)btn.Tag;

    Dictionary<object, int> dct = stat.Records.ToDictionary(r => r.Key, r => r.Count);
    FrmStatsDictView frm = new FrmStatsDictView();
    frm.SetUp($"Details of '{stat.Stat.Title}'", dct);
    frm.Show();
  }
}
