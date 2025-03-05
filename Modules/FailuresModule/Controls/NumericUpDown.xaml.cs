using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

namespace Eng.EFsExtensions.Modules.FailuresModule.Controls
{
  /// <summary>
  /// Interaction logic for NumericUpDown.xaml
  /// </summary>
  public partial class NumericUpDown : UserControl
  {
    public NumericUpDown()
    {
      InitializeComponent();
    }

    public static readonly DependencyProperty ValueProperty =
   DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericUpDown));

    public double Value
    {
      get
      {
        double ret = (double)GetValue(ValueProperty);
        return ret;
      }
      set
      {
        SetValue(ValueProperty, value);
      }
    }

    private (int, int) GetValAndOrder(double d)
    {
      int order = 0;
      int value = 0;

      d = Math.Abs(d);
      if (d >= 1)
      {
        while(d >= 1)
        {
          order++;
          d /= 10;
        }
        d *= 10;
      }
      else
      {
        do
        {
          order--;
          d *= 10;
        } while (d < 1);
      }
      value = (int)(d % 10);
      order--;
      return (value, order);
    }

    public void IncreaseValue()
    {
      int v, o;
      (v, o) = GetValAndOrder(this.Value);
      double inc = (v + 1) * Math.Pow(10, o);
      Value = inc;
    }

    public void DecreaseValue()
    {
      int v, o;
      (v, o) = GetValAndOrder(this.Value);
      double dec;
      if (v == 1)
        dec = 9 * Math.Pow(10, o - 1);
      else
        dec = (v - 1) * Math.Pow(10, o);
      Value = dec;
    }

    private void btnUp_Click(object sender, RoutedEventArgs e)
    {
      IncreaseValue();
    }

    private void btnDown_Click(object sender, RoutedEventArgs e)
    {
      DecreaseValue();
    }
  }
}
