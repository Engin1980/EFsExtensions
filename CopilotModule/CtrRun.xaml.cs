using Eng.Chlaot.Modules.CopilotModule;
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

namespace CopilotModule
{
  /// <summary>
  /// Interaction logic for CtrRun.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private RunContext runContext;

    public CtrRun()
    {
      InitializeComponent();
      this.runContext = null!;
    }

    public CtrRun(RunContext runContext)
    {
      this.runContext = runContext;
    }
  }
}
