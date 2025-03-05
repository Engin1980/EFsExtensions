using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi
{
  /// <summary>
  /// Interaction logic for CtrSettings.xaml
  /// </summary>
  public partial class CtrSettings : UserControl
  {
    private readonly MsSapiSettings settings;
    public CtrSettings()
    {
      InitializeComponent();
      this.DataContext = this.settings = null!;
    }

    public CtrSettings(MsSapiSettings settings)
    {
      InitializeComponent();
      this.DataContext = this.settings = settings;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
      e.Handled = true;
    }
  }
}
