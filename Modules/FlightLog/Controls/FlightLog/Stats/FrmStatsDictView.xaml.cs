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

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.Stats
{
  /// <summary>
  /// Interaction logic for FrmStatsDictView.xaml
  /// </summary>
  public partial class FrmStatsDictView : Window
  {
    public class Item : NotifyPropertyChanged
    {
      public int Index
      {
        get => base.GetProperty<int>(nameof(Index));
        set => base.UpdateProperty(nameof(Index), value);
      }
      public object Key
      {
        get => base.GetProperty<object>(nameof(Key)) ?? string.Empty;
        set => base.UpdateProperty(nameof(Key), value);
      }
      public int Count
      {
        get => base.GetProperty<int>(nameof(Count));
        set => base.UpdateProperty(nameof(Count), value);
      }
    }
    public class FrmStatsDictViewViewModel : NotifyPropertyChanged
    {
      public string Title
      {
        get => base.GetProperty<string>(nameof(Title)) ?? string.Empty;
        set => base.UpdateProperty(nameof(Title), value);
      }
      public List<Item> Items
      {
        get => base.GetProperty<List<Item>>(nameof(Items)) ?? [];
        set => base.UpdateProperty(nameof(Items), value);
      }
    }

    private readonly FrmStatsDictViewViewModel vm;

    public FrmStatsDictView()
    {
      InitializeComponent();
      this.DataContext = vm = new();
    }

    internal void SetUp(string title, Dictionary<object, int> dct)
    {
      this.vm.Title = title;
      int index = 1;
      this.vm.Items = dct.Select(kv => new Item() { Index = index++, Key = kv.Key, Count = kv.Value }).ToList();
    }
  }
}
