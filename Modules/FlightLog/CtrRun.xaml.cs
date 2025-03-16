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


    private readonly RunContext Context = null!;
    private readonly NewSimObject simObject = null!;
    

    private RunContext context;


    public CtrRun()
    {
      InitializeComponent();
    }

    public CtrRun(InitContext initContext) : this()
    {
      this.simObject = NewSimObject.GetInstance();
      this.simPropValues = new SimPropValues(this.simObject.ExtValue);
      this.DataContext = this.Context = new RunContext(initContext);
    }

    public void Start()
    {
      this.simObject.ExtTime.SimSecondElapsed += () => this.context.ProcessSecondElapsed();
    }    
  }
}
