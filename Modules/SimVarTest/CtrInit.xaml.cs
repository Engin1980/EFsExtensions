using Eng.Chlaot.Modules.SimVarTestModule;
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

namespace Eng.Chlaot.Modules.SimVarTestModule
{
  /// <summary>
  /// Interaction logic for CtrInit.xaml
  /// </summary>
  public partial class CtrInit : UserControl
  {
    public CtrInit()
    {
      InitializeComponent();
    }

    public CtrInit(Context context) : this()
    {
      DataContext = context;
    }
  }
}
