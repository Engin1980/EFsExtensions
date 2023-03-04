using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Chlaot
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class FrmInit : Window
  {
    private readonly Context context = new Context();

    public FrmInit()
    {
      InitializeComponent();
    }

    [SuppressMessage("", "IDE1006")]
    private void btnRun_Click(object sender, RoutedEventArgs e)
    {
      FrmRun frmRun = new(this.context);
      Logger.UnregisterLogAction(this);
      this.Close();
      frmRun.Show();
    }

    [SuppressMessage("", "IDE1006")]
    private void lstModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      IModule module = (IModule)lstModules.SelectedItem;
      pnlContent.Children.Clear();
      pnlContent.Children.Add(module.InitControl);
    }

    private void Window_Initialized(object sender, EventArgs e)
    {
      LogHelper.RegisterGlobalLogListener();
      LogHelper.RegisterWindowLogListener(this,this.txtConsole);
      this.context.SetUpModules();

      this.DataContext = this.context;
      if (lstModules.Items.Count > 0) lstModules.SelectedIndex = 0;

      this.context.InitModules();
    }

    


  }
}
