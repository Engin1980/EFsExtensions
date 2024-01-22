using Eng.Chlaot.Modules.FailuresModule.Model.Incidents;
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

namespace Eng.Chlaot.Modules.FailuresModule.Controls
{
  /// <summary>
  /// Interaction logic for ProbabilityUpDown.xaml
  /// </summary>
  public partial class ProbabilityUpDown : UserControl
  {
    public ProbabilityUpDown()
    {
      InitializeComponent();
    }

    public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value), typeof(Percentage), typeof(ProbabilityUpDown));

    public Percentage Value
    {
      get
      {
        Percentage ret = (Percentage)GetValue(ValueProperty);
        return ret;
      }
      set
      {
        double v = Math.Max(0, Math.Min(1, value));
        Percentage p = (Percentage)v;
        SetValue(ValueProperty, p);
      }
    }

    public void IncreaseValue()
    {
      double value = Value;
      if (value < 0.001)
        value += 0.0001;
      else if (value < 0.01)
        value += 0.001;
      else if (value < 0.1)
        value += 0.01;
      else
        value += 0.1;
      value = Math.Round(value, 4);
      Value = (Percentage)value;
    }

    public void DecreaseValue()
    {
      double value = Value;
      if (value <= 0.001)
        value -= 0.0001;
      else if (value <= 0.01)
        value -= 0.001;
      else if (value <= 0.1)
        value -= 0.01;
      else
        value -= 0.1;
      value = Math.Round(value, 4);
      Value = (Percentage)value;
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
