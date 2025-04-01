using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Exceptions;
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
  /// Interaction logic for GpsLabel.xaml
  /// </summary>
  public partial class GpsLabel : UserControl
  {
    public enum GpsNumericFormat
    {
      Decimal,
      DMS
    }
    public enum GpsDirectionFormat
    {
      Sign,
      Char
    }

    private readonly static DependencyProperty NumericFormatProperty = DependencyProperty.Register(
      nameof(NumericFormat), typeof(GpsNumericFormat), typeof(GpsLabel), new PropertyMetadata(GpsNumericFormat.Decimal, OnValueChanged));
    private readonly static DependencyProperty DirectionFormatProperty = DependencyProperty.Register(
      nameof(DirectionFormat), typeof(GpsDirectionFormat), typeof(GpsLabel), new PropertyMetadata(GpsDirectionFormat.Sign, OnValueChanged));
    private readonly static DependencyProperty LatitudeProperty = DependencyProperty.Register(
      nameof(Latitude), typeof(double?), typeof(GpsLabel), new PropertyMetadata(null, OnValueChanged));
    private readonly static DependencyProperty LongitudeProperty = DependencyProperty.Register(
      nameof(Longitude), typeof(double?), typeof(GpsLabel), new PropertyMetadata(null, OnValueChanged));
    private readonly static DependencyProperty GpsProperty = DependencyProperty.Register(
      nameof(Gps), typeof(GPS?), typeof(GpsLabel), new PropertyMetadata(null, OnValueChanged));
    private static readonly DependencyProperty DisplayValueProperty = DependencyProperty.Register(
      nameof(DisplayValue), typeof(string), typeof(GpsLabel), new PropertyMetadata(string.Empty));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is GpsLabel tsl)
        tsl.UpdateDisplayValue();
    }

    public GpsNumericFormat NumericFormat
    {
      get => (GpsNumericFormat)GetValue(NumericFormatProperty);
      set => SetValue(NumericFormatProperty, value);
    }

    public GpsDirectionFormat DirectionFormat
    {
      get => (GpsDirectionFormat)GetValue(DirectionFormatProperty);
      set => SetValue(DirectionFormatProperty, value);
    }

    public double? Latitude
    {
      get => (double?)GetValue(LatitudeProperty);
      set => SetValue(LatitudeProperty, value);
    }

    public double? Longitude
    {
      get => (double?)GetValue(LongitudeProperty);
      set => SetValue(LongitudeProperty, value);
    }

    public GPS? Gps
    {
      get => (GPS?)GetValue(GpsProperty);
      set => SetValue(GpsProperty, value);
    }

    public string DisplayValue
    {
      get => (string)GetValue(DisplayValueProperty);
      set => SetValue(DisplayValueProperty, value);
    }

    private void UpdateDisplayValue()
    {
      const string NUMBER_FORMAT = "N5";
      const string DELIMITER = ", ";

      double lat = this.Gps?.Latitude ?? this.Latitude ?? double.NaN;
      double lon = this.Gps?.Longitude ?? this.Longitude ?? double.NaN;
      string tmp;
      if (double.IsNaN(lat) || double.IsNaN(lon))
        tmp = string.Empty;
      else if (NumericFormat == GpsNumericFormat.Decimal)
      {
        if (DirectionFormat == GpsDirectionFormat.Sign)
        {
          tmp = $"{lat.ToString(NUMBER_FORMAT)}{DELIMITER}{lon.ToString(NUMBER_FORMAT)}";
        }
        else if (DirectionFormat == GpsDirectionFormat.Char)
        {
          tmp = $"{lat.ToString(NUMBER_FORMAT)} {(lat < 0 ? 'S' : 'N')}{DELIMITER}{lon.ToString(NUMBER_FORMAT)} {(lon < 0 ? 'W' : 'E')}";
        }
        else
          throw new UnexpectedEnumValueException(this.DirectionFormat);
      }
      else if (NumericFormat == GpsNumericFormat.DMS)
      {
        static (double d, double m, double s) decodeDMS(double dec)
        {
          double d = Math.Floor(dec);
          double m = Math.Floor((dec - d) * 60);
          double s = (dec - d - m / 60) * 3600;
          return (d, m, s);
        }

        double latD, latM, latS, lonD, lonM, lonS;
        (latD, latM, latS) = decodeDMS(lat);
        (lonD, lonM, lonS) = decodeDMS(lon);
        if (DirectionFormat == GpsDirectionFormat.Sign)
        {
          tmp = $"{latD}°{latM}'{latS}\"{DELIMITER}{lonD}°{lonM}'{lonS}\"";
        }
        else if (DirectionFormat == GpsDirectionFormat.Char)
        {
          tmp = $"{latD}°{latM}'{latS}{DELIMITER}{lonD}\" {(lat < 0 ? 'S' : 'N')}°{lonM}'{lonS}\" {(lon < 0 ? 'W' : 'E')}";
        }
        else
          throw new UnexpectedEnumValueException(this.DirectionFormat);
      }
      else
        throw new UnexpectedEnumValueException(this.NumericFormat);
      this.DisplayValue = tmp;
    }

    public GpsLabel()
    {
      InitializeComponent();
      UpdateDisplayValue();
    }
  }
}
