using Eng.EFsExtensions.Modules.AffinityModule;
using Microsoft.WindowsAPICodePack.ShellExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Eng.EFsExtensions.Modules.AffinityModule
{
  /// <summary>
  /// Interaction logic for CtrRun.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private readonly Context context;
    private DateTime lastAdjustmentCompletedTime = DateTime.Now;
    public CtrRun()
    {
      InitializeComponent();
      this.context = null!;
    }
    private void AdjustmentCompleted()
    {
      List<ProcessAdjustResult> changed = new();
      List<ProcessAdjustResult> unchanged = new();
      List<ProcessAdjustResult> fails = new();
      List<ProcessAdjustResult> unmatched = new();
      var fail = ProcessAdjustResult.EResult.Failed;
      var unch = ProcessAdjustResult.EResult.Unchanged;

      foreach (ProcessAdjustResult info in this.context.ProcessInfos)
      {
        if (info.AffinityRule == null && info.PriorityRule == null)
          unmatched.Add(info);
        else if (info.AffinitySetResult == fail || info.AffinityGetResult == fail
          || info.PrioritySetResult == fail || info.PriorityGetResult == fail)
          fails.Add(info);
        else if (info.AffinitySetResult == unch && info.PrioritySetResult == unch)
          unchanged.Add(info);
        else
          changed.Add(info);
      }

      this.grdProcessed.ItemsSource = changed;
      this.tabProcessed.Header = $"Processed items ({changed.Count})";

      this.grdFailed.ItemsSource = fails;
      this.tabFailed.Header = $"Failed items ({fails.Count})";

      this.grdSkipped.ItemsSource = unchanged;
      this.tabSkipped.Header = $"Skipped items ({unchanged.Count})";

      this.grdUnmatched.ItemsSource = unmatched;
      this.tabUnmatched.Header = $"Unmatched items ({unmatched.Count})";
    }

    public CtrRun(Context context) : this()
    {
      this.context = context;
      this.DataContext = context;
      this.context.SingleProcessAdjustmentCompleted += Context_SingleProcessAdjustmentCompleted;
      this.context.AllProcessesAdjustmentCompleted += Context_AllProcessesAdjustmentCompleted; ;
    }

    private void Context_AllProcessesAdjustmentCompleted()
    {
      lastAdjustmentCompletedTime = DateTime.Now;
      Dispatcher.Invoke(AdjustmentCompleted);
    }

    private void Context_SingleProcessAdjustmentCompleted(ProcessAdjustResult processAdjustResult)
    {
      if ((DateTime.Now - lastAdjustmentCompletedTime).TotalSeconds > 1)
      {
        lastAdjustmentCompletedTime = DateTime.Now;
        Dispatcher.Invoke(AdjustmentCompleted);
      }
    }
  }
}
