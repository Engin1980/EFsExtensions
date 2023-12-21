using FailuresModule.Types.Run.Sustainers;
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

namespace FailuresModule
{
  /// <summary>
  /// Interaction logic for CtrRun.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private RunContext context;

    public CtrRun()
    {
      InitializeComponent();
    }

    public CtrRun(RunContext context) : this()
    {
      this.context = context;
      this.DataContext = context;
    }

    private void btnToggleSustainerActive_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      FailureSustainer fs = (FailureSustainer)btn.Tag;
      fs.Toggle();
    }
  }
}
