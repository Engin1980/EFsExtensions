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

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.MSAPI
{
  /// <summary>
  /// Interaction logic for CtrSettings.xaml
  /// </summary>
  public partial class CtrSettings : UserControl
  {
    private readonly MSapiSettings settings;
    public CtrSettings()
    {
      InitializeComponent();
      this.DataContext = this.settings = null!;
    }

    public CtrSettings(MSapiSettings settings)
    {
      InitializeComponent();
      this.DataContext = this.settings = settings;
    }
  }
}
