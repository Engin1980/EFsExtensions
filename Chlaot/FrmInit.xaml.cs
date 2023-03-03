using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public void LogToConsole(LogLevel level, string message)
    {
      if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
        Application.Current.Dispatcher.Invoke(new Action(() => { this.LogToConsole(level, message); }));
      else
      {
        if (level != LogLevel.VERBOSE)
        {
          txtConsole.AppendText("\n");
          txtConsole.AppendText(level + ":: " + message);
          txtConsole.ScrollToEnd();
        }
        try
        {
          System.IO.File.AppendAllText("log.txt",
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {level} :: {message}\n");
        }
        catch
        {
          txtConsole.AppendText("\n");
          txtConsole.AppendText(LogLevel.WARNING + ":: Failed to write message to log file.");
          txtConsole.ScrollToEnd();
        }
      }
    }

    [SuppressMessage("", "IDE1006")]
    private void btnRun_Click(object sender, RoutedEventArgs e)
    {
      FrmRun frmRun = new(this.context);
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
      this.context.SetLogHandler((l, m) => this.LogToConsole(l, m));
      this.context.SetUpModules();

      this.DataContext = this.context;
      if (lstModules.Items.Count > 0) lstModules.SelectedIndex = 0;

      this.context.InitModules();
    }
  }
}
