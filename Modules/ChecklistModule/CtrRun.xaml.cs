using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.KeyHooking;
using Eng.EFsExtensions.Modules.ChecklistModule;
using Eng.EFsExtensions.Modules.ChecklistModule.Types.VM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ChecklistModule
{
  /// <summary>
  /// Interaction logic for RunControl.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private readonly RunContext context = null!;

    public CtrRun()
    {
      InitializeComponent();
    }

    internal CtrRun(RunContext context) : this()
    {
      this.DataContext = this.context = context;

      var keyHookWrapper = new KeyHookWrapper();
      this.context.Run(keyHookWrapper);
    }

    private void btnSetAsCurrentChecklist_Click(object sender, RoutedEventArgs e)
    {
      CheckListVM vm = (((MenuItem)sender).Tag as CheckListVM)!;
      this.context.SetCurrentChecklist(vm);
    }
  }
}
