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
    private static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
      nameof(Value), typeof(TimeSpan?), typeof(TimeSpanLabel), new PropertyMetadata(null, OnValueChanged));

    public TimeSpan? Value
    {
      get => (TimeSpan?)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
    }

    private static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
      nameof(StringFormat), typeof(string), typeof(TimeSpanLabel), new PropertyMetadata(null, OnValueChanged));

    public string? StringFormat
    {
      get => (string?)GetValue(StringFormatProperty);
      set => SetValue(StringFormatProperty, value);
    }

    private static readonly DependencyProperty DisplayValueProperty = DependencyProperty.Register(
      nameof(DisplayValue), typeof(string), typeof(TimeSpanLabel), new PropertyMetadata(string.Empty));

    public string DisplayValue
    {
      get => (string)GetValue(DisplayValueProperty);
      set => SetValue(DisplayValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is TimeSpanLabel tsl)
        tsl.UpdateDisplayValue();
    }

    private void UpdateDisplayValue()
    {
      if (StringFormat == null || Value == null)
        this.DisplayValue = string.Empty;
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
          .Replace("ff", (Value.Value.Milliseconds/10).ToString("00"))
          .Replace("f", (Value.Value.Milliseconds/100).ToString());
        this.DisplayValue = tmp;
      }
    }

    public TimeSpanLabel()
    {
      InitializeComponent();
      this.UpdateDisplayValue();
    }
  }
}
