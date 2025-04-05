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
      nameof(Value), typeof(TimeSpan?), typeof(TimeSpanLabel), new PropertyMetadata(null));

    public TimeSpan? Value
    {
      get => (TimeSpan?)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
    }

    private static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
      nameof(StringFormat), typeof(string), typeof(TimeSpanLabel), new PropertyMetadata("d. h:mm:ss"));

    public string? StringFormat
    {
      get => (string?)GetValue(StringFormatProperty);
      set => SetValue(StringFormatProperty, value);
    }

    public TimeSpanLabel()
    {
      InitializeComponent();
    }
  }
}
