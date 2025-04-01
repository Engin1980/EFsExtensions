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

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.Shared
{
  /// <summary>
  /// Interaction logic for TimeSpanLabel.xaml
  /// </summary>
  public partial class TimeSpanLabel : UserControl
  {
    private static DependencyProperty ValueProperty = DependencyProperty.Register(
      "Value", typeof(TimeSpan), typeof(TimeSpanLabel), null);

    public TimeSpan? Value
    {
      get => (TimeSpan?)GetValue(ValueProperty);
      set
      {
        SetValue(ValueProperty, value);
        UpdateDisplayValue();
      }
    }

    private static DependencyProperty StringFormatProperty = DependencyProperty.Register(
      "StringFormat", typeof(string), typeof(TimeSpanLabel), null);

    public string? StringFormat
    {
      get => (string?)GetValue(StringFormatProperty);
      set
      {
        SetValue(StringFormatProperty, value);
        UpdateDisplayValue();
      }
    }

    private void UpdateDisplayValue()
    {
      if (StringFormat == null || Value == null)
        this.DataContext = "(nulls)";
      else
      {
        string sf = this.StringFormat;
        string tmp = sf
          .Replace("ddd", Value.Value.Days.ToString("000"))
          .Replace("dd", Value.Value.Days.ToString("00"))
          .Replace("d", Value.Value.Days.ToString())
          .Replace("hh", Value.Value.Hours.ToString("00"))
          .Replace("h", Value.Value.Hours.ToString())
          .Replace("mm", Value.Value.Minutes.ToString("00"))
          .Replace("m", Value.Value.Minutes.ToString())
          .Replace("ss", Value.Value.Seconds.ToString("00"))
          .Replace("s", Value.Value.Seconds.ToString())
          .Replace("fff", Value.Value.Milliseconds.ToString("000"))
          .Replace("ff", Value.Value.Milliseconds.ToString("00"))
          .Replace("f", Value.Value.Milliseconds.ToString());
        this.DataContext = tmp;
      }
    }

    public TimeSpanLabel()
    {
      InitializeComponent();
      this.UpdateDisplayValue();
    }
  }
}
