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
using System.Windows.Shapes;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.LogFlightOverview
{
  /// <summary>
  /// Interaction logic for FrmVisibleColumns.xaml
  /// </summary>
  public partial class FrmVisibleColumns : Window
  {
    public class ColumnVisibility : NotifyPropertyChanged
    {
      public string Title
      {
        get => base.GetProperty<string>(nameof(Title))!;
        set => base.UpdateProperty(nameof(Title), value);
      }
      public bool IsVisible
      {
        get => base.GetProperty<bool>(nameof(IsVisible))!;
        set => base.UpdateProperty(nameof(IsVisible), value);
      }
    }

    public void Init(Dictionary<string, bool> columnsVisibility)
    {
      this.DataContext = columnsVisibility
        .Select(q => new ColumnVisibility()
        {
          Title = q.Key,
          IsVisible = q.Value
        })
        .ToList();
    }

    internal Dictionary<string, bool> GetResultDictionary()
    {
      Dictionary<string, bool> ret = ((List<ColumnVisibility>)this.DataContext)
        .ToDictionary(ColumnVisibility => ColumnVisibility.Title,
                      ColumnVisibility => ColumnVisibility.IsVisible);
      return ret;
    }

    public FrmVisibleColumns()
    {
      InitializeComponent();
      this.DataContext = new List<ColumnVisibility>();
    }
  }
}
