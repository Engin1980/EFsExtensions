using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ChecklistModule
{
  /// <summary>
  /// Interaction logic for RunControl.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private readonly RunContext context;

    public CtrRun()
    {
      InitializeComponent();
    }

    public CtrRun(RunContext context) : this()
    {
      this.context = context;
      this.context.CurrentChanged += Context_CurrentChanged;
      this.XC = new XCo(this.context);
      this.DataContext = context;
    }

    private void Context_CurrentChanged()
    {
      //WalkDownLogicalTree(tvwTree);
    }


    //void WalkDownLogicalTree(object current)
    //{
    //  AdjustStyleIfRequired(current);

    //  // The logical tree can contain any type of object, not just 
    //  // instances of DependencyObject subclasses.  LogicalTreeHelper
    //  // only works with DependencyObject subclasses, so we must be
    //  // sure that we do not pass it an object of the wrong type.
    //  DependencyObject depObj = current as DependencyObject;

    //  if (depObj != null)
    //    foreach (object logicalChild in LogicalTreeHelper.GetChildren(depObj))
    //      WalkDownLogicalTree(logicalChild);
    //}

    //private void AdjustStyleIfRequired(object current)
    //{
    //  if (current is Label lbl)
    //  {
    //    if (lbl.Tag == this.context.CurrentChecklist || lbl.Tag == this.context.CurrentCheckItem)
    //    {
    //      lbl.Background = new SolidColorBrush(Colors.Yellow);
    //    }
    //  }
    //}

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      Context_CurrentChanged();
    }

    public XCo XC { get; set; }

    public class XCo : IValueConverter
    {
      private RunContext context;
      public XCo(RunContext context)
      {
        this.context = context;
      }
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
        SolidColorBrush ret;
        if (value == context.CurrentChecklist)
        {
          ret = new SolidColorBrush(Colors.Yellow);
        }
        else
          ret = new SolidColorBrush(Colors.White);
        return ret;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
        throw new NotImplementedException();
      }
    }
  }
}
