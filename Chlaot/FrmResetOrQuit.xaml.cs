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
  /// Interaction logic for FrmResetOrQuit.xaml
  /// </summary>
  public partial class FrmResetOrQuit : Window
  {
    public enum ResetQuitDialogResult
    {
      Cancel,
      Init,
      InitAndReset,
      Quit
    }

    public new ResetQuitDialogResult DialogResult { get; set; } = ResetQuitDialogResult.Cancel;

    public FrmResetOrQuit()
    {
      InitializeComponent();
    }

    private void CloseWithResult(ResetQuitDialogResult result)
    {
      this.DialogResult = result;
      this.Close();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      CloseWithResult(ResetQuitDialogResult.Cancel);
    }

    private void btnInit_Click(object sender, RoutedEventArgs e)
    {
      CloseWithResult(ResetQuitDialogResult.Init);
    }

    private void btnInitReset_Click(object sender, RoutedEventArgs e)
    {
      CloseWithResult(ResetQuitDialogResult.InitAndReset);
    }

    private void btnQuit_Click(object sender, RoutedEventArgs e)
    {
      CloseWithResult(ResetQuitDialogResult.Quit);
    }
  }
}
