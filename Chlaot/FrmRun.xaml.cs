using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Chlaot
{
  /// <summary>
  /// Interaction logic for FrmRun.xaml
  /// </summary>
  public partial class FrmRun : Window
  {
    private readonly Context context;
    public FrmRun()
    {
      InitializeComponent();
      this.context = null!;
    }

    public FrmRun(Context context) : this()
    {
      this.context = context ?? throw new ArgumentNullException(nameof(context));
      this.DataContext = context;
    }

    private void lstModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      IModule module = (IModule)lstModules.SelectedItem;
      pnlContent.Children.Clear();
      pnlContent.Children.Add(module.RunControl);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      LogHelper.RegisterWindowLogListener(this, this.txtConsole);
      this.context.RemoveUnreadyModules();
      this.DataContext = this.context;
      this.context.RunModules();
      if (lstModules.Items.Count > 0) lstModules.SelectedIndex = 0;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      Logger.UnregisterLogAction(this);
      foreach (var module in context.Modules)
      {
        module.Stop();
      }
      Application.Current.Shutdown();
    }
  }
}
