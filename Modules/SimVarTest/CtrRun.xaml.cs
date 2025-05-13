using Eng.EFsExtensions.Modules.SimVarTestModule.Controls;
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

namespace Eng.EFsExtensions.Modules.SimVarTestModule
{
  /// <summary>
  /// Interaction logic for CtrRun.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private readonly Context context;

    public CtrRun()
    {
      this.context = null!;
      InitializeComponent();
    }

    public CtrRun(Context context) : this()
    {
      this.context = context;
      this.DataContext = context;

      this.context.Connect();
    }

    private void btnNewSimVar_Click(object sender, RoutedEventArgs e)
    {
      string name = txtNewSimVar.Text;
      txtNewSimVar.Text = "";

      bool validate = chkNewSimVarValidate.IsChecked == true;

      this.context.RegisterNewSimVar(name, validate);
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      SimVarCase svc = (SimVarCase)btn.Tag;

      this.context.DeleteSimVar(svc);
    }

    private void txtSimEventValue_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.RightButton != MouseButtonState.Pressed) return;
      TextBlock txt = (TextBlock)sender;
      txtSimEvent.Text = txt.Text;
      tab.SelectedIndex = 2;
    }

    private void txtSimVarValue_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.RightButton != MouseButtonState.Pressed) return;
      TextBlock txt = (TextBlock)sender;
      txtNewSimVar.Text = txt.Text;
      tab.SelectedItem = tabSimVars;
    }

    private void txtAppliedSimVar_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.RightButton != MouseButtonState.Pressed) return;
      TextBlock txt = (TextBlock)sender;
      txtSimEvent.Text = txt.Text;
      tab.SelectedIndex = 2;
    }

    private void btnSendSimEvent_Click(object sender, RoutedEventArgs e)
    {
      string txt = txtSimEvent.Text;
      this.context.SendEvent(txt);
      this.context.AppliedSimEvents.Add(txt);
    }

    internal void NewValue_NewValueRequested(NewValue sender, double newValue)
    {
      // used in XAML
      SimVarCase simVarCase = (SimVarCase)sender.Tag;
      this.context.SetValue(simVarCase, newValue);
    }
  }
}
