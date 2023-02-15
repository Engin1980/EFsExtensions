using Eng.Chlaot.ChlaotModuleBase;
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

namespace Chlaot
{
  /// <summary>
  /// Interaction logic for FrmRun.xaml
  /// </summary>
  public partial class FrmRun : Window
  {
    private Context Context { get; set; }
    public FrmRun()
    {
      InitializeComponent();
    }

    public FrmRun(Context context) : this()
    {
      this.Context = context;
      this.Context.SetLogHandler((l, m) => LogToConsole(l, m));
      RemoveUnreadyModules();
      this.DataContext = this.Context;
      StartModules();
    }

    private void StartModules()
    {
      foreach (var module in Context.Modules)
      {
        module.Start();
        LogToConsole(LogLevel.INFO, $"Module '{module.Name}' started.");
      }
    }

    private void RemoveUnreadyModules()
    {
      var unready = this.Context.Modules.Where(q => q.IsReady == false).ToList();
      foreach (var module in unready)
      {
        this.Context.Modules.Remove(module);
        LogToConsole(LogLevel.INFO, $"Module '{module.Name}' removed, not ready.");
      }
    }

    private void lstModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      IModule module = lstModules.SelectedItem as IModule;
      pnlContent.Children.Clear();
      pnlContent.Children.Add(module.RunControl);
    }

    private void LogToConsole(LogLevel level, string message)
    {
      txtConsole.AppendText("\n");
      txtConsole.AppendText(level + ":: " + message);
      txtConsole.ScrollToEnd();
    }
  }
}
