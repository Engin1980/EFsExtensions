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
      List<ProcessInfo> tmp;

      tmp = this.context.ProcessInfos
        .Where(q => q.IsAccessible == true)
        .OrderBy(q => q.Name)
        .ToList();
      this.grdProcessed.ItemsSource = tmp;
      this.tabProcessed.Header = $"Processed items ({tmp.Count})";

      tmp = this.context.ProcessInfos
          .Where(q => q.IsAccessible == false)
          .OrderBy(q => q.Name)
          .ToList();
      this.grdFailed.ItemsSource = tmp;
      this.tabFailed.Header = $"Failed items ({tmp.Count})";

      tmp = this.context.ProcessInfos
        .Where(q => q.IsAccessible == null)
        .OrderBy(q => q.Name)
        .ToList();
      this.grdSkipped.ItemsSource = tmp;
      this.tabSkipped.Header = $"Skipped items ({tmp.Count})";
    }

    public CtrRun(Context context) : this()
    {
      this.context = context;
      this.DataContext = context;
      this.context.AdjustmentCompleted += AdjustmentCompletedWrapped;

    }
  }
}
