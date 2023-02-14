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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chlaot
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class FrmInit : Window
  {
    public Context Context { get; set; }

    public FrmInit()
    {
      InitializeComponent();
      InitializeContext();

      this.DataContext = this.Context;
    }

    public void InitializeContext()
    {
      this.Context = new Context(
        (l, s) => LogToConsole(l, s));
      this.Context.InitModules();
    }

    public void LogToConsole(LogLevel level, string message)
    {
      txtConsole.AppendText("\n");
      txtConsole.AppendText(level + ":: " + message);

    }

    private void lstModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      IModule module = lstModules.SelectedItem as IModule;
      pnlContent.Children.Clear();
      pnlContent.Children.Add(module.InitControl);
    }
  }
}
