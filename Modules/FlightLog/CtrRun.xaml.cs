using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using ESimConnect;
using ESystem.Miscelaneous;
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

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  /// <summary>
  /// Interaction logic for CtrRun.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private readonly NewSimObject simObject = null!;
    private readonly RunContext context = null!;

    public CtrRun()
    {
      InitializeComponent();
    }

    public CtrRun(InitContext initContext, Settings settings) : this()
    {
      this.simObject = NewSimObject.GetInstance();
      this.DataContext = this.context = new RunContext(initContext, this.simObject.ExtValue, settings);
    }

    public void Start()
    {
      this.simObject.ExtTime.SimSecondElapsed += () => this.context.ProcessSecondElapsed();
    }
  }
}
