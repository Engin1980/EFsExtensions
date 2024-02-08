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

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs
{
  /// <summary>
  /// Interaction logic for CtrTtss.xaml
  /// </summary>
  public partial class CtrTtss : UserControl
  {
    public CtrTtss()
    {
      InitializeComponent();

      tabTtss.SelectionChanged += TabTtss_SelectionChanged;
      tabTtss.Items.Add(new TabItem()
      {
        Header = "Not initialized",
        Content = new Label()
        {
          Content = "Not initialized"
        }
      });

    }

    public ITtsModule SelectedModule
    {
      get { return (ITtsModule)GetValue(SelectedModuleProperty); }
      set { SetValue(SelectedModuleProperty, value); }
    }

    public static readonly DependencyProperty SelectedModuleProperty =
        DependencyProperty.Register(nameof(SelectedModule), typeof(ITtsModule), typeof(CtrTtss));

    public void Init(IEnumerable<ITtsModule> modules)
    {
      tabTtss.Items.Clear();
      foreach (var module in modules)
      {
        DockPanel dck = new DockPanel();
        dck.Children.Add(module.SettingsControl);

        TabItem tabItem = new()
        {
          Header = module.Name,
          Content = dck,
          Tag = module
        };
        tabTtss.Items.Add(tabItem);

      }
    }

    private void TabTtss_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (tabTtss.SelectedItem is TabItem ti)
        this.SelectedModule = (ITtsModule)ti.Tag;
    }
  }
}
