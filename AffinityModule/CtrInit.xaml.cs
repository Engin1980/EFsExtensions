using ChlaotModuleBase;
using ELogging;
using Eng.Chlaot.Modules.AffinityModule;
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

namespace AffinityModule
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class CtrInit : System.Windows.Controls.UserControl
  {
    public readonly NewLogHandler logHandler;
    public readonly Context context;
    public CtrInit()
    {
      InitializeComponent();
      this.logHandler = null!;
      this.context = null!;
    }

    public CtrInit(Context context) : this()
    {
      this.logHandler = Logger.RegisterSender(this);
      this.context = context;
      this.DataContext = context;
    }

    private void btnAddRule_Click(object sender, RoutedEventArgs e)
    {
      context.Settings.Rules.AddNew();
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      this.context.SaveSettings();
    }

    private void btnLoad_Click(object sender, RoutedEventArgs e)
    {
      this.context.LoadSettings();
    }
  }
}