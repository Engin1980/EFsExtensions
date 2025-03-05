using Eng.EFsExtensions.Modules.CopilotModule;
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
    private readonly RunContext runContext;

    public CtrRun()
    {
      InitializeComponent();
      this.runContext = null!;
    }

    internal CtrRun(RunContext runContext) : this()
    {
      this.runContext = runContext;
      this.DataContext = runContext;
    }
  }
}
