using Eng.Chlaot.Modules.AffinityModule;
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

namespace AffinityModule
{
  /// <summary>
  /// Interaction logic for CtrRun.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private readonly Context context;
    public CtrRun()
    {
      InitializeComponent();
      this.context = null!;
    }
    private void AdjustmentCompletedWrapped()
    {
      Application.Current.Dispatcher.Invoke(() => AdjustmentCompleted());
    }
    private void AdjustmentCompleted()
    {
      List<ProcessAdjustResult> oks = new();
      List<ProcessAdjustResult> fails = new();
      List<ProcessAdjustResult> skips = new();

      foreach (ProcessAdjustResult info in this.context.ProcessInfos)
      {
        if (info.AffinitySetResult == ProcessAdjustResult.EResult.Unchanged
          && info.PrioritySetResult == ProcessAdjustResult.EResult.Unchanged)
          skips.Add(info);
        else if (info.AffinitySetResult == ProcessAdjustResult.EResult.Failed
          || info.PrioritySetResult == ProcessAdjustResult.EResult.Failed)
          fails.Add(info);
        else
          oks.Add(info);
      }

      this.grdProcessed.ItemsSource = oks;
      this.tabProcessed.Header = $"Processed items ({oks.Count})";
      
      this.grdFailed.ItemsSource = fails;
      this.tabFailed.Header = $"Failed items ({fails.Count})";

      this.grdSkipped.ItemsSource = skips;
      this.tabSkipped.Header = $"Skipped items ({skips.Count})";
    }

    public CtrRun(Context context) : this()
    {
      this.context = context;
      this.DataContext = context;
      this.context.AdjustmentCompleted += AdjustmentCompletedWrapped;

    }
  }
}
