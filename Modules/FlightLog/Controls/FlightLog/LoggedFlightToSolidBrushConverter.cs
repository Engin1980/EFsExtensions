using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  public class LoggedFlightToSolidBrushConverter : TypedConverter<LoggedFlight, Brush>
  {
    public readonly static Brush[] predefinedBrushes = new Brush[] {
        Brushes.Black,
        Brushes.DarkBlue,
        Brushes.DarkGreen,
        Brushes.Maroon,
        Brushes.DarkRed,
        Brushes.DarkCyan,
        Brushes.DarkMagenta,
        Brushes.MidnightBlue,
        Brushes.Olive,
        Brushes.Indigo,
        Brushes.Teal,
        Brushes.SaddleBrown,
        Brushes.SeaGreen,
        Brushes.Firebrick,
        Brushes.RoyalBlue,
        Brushes.ForestGreen,
        Brushes.SteelBlue,
        Brushes.Purple,
        Brushes.Brown,
        Brushes.DarkSlateGray,
        Brushes.DarkOliveGreen,
        Brushes.Crimson,
        Brushes.MediumBlue,
        Brushes.MediumVioletRed,
        Brushes.MediumSlateBlue,
        Brushes.DarkOrchid,
        Brushes.BlueViolet,
        Brushes.DarkGoldenrod,
        Brushes.Chocolate,
        Brushes.MediumSeaGreen,
        Brushes.IndianRed,
        Brushes.DodgerBlue
    };

    protected override Brush Convert(LoggedFlight value, object parameter, CultureInfo culture)
    {
      string s = value.AircraftRegistration ?? string.Empty;
      int i = s.Sum(q => q);
      i = Math.Abs(i % predefinedBrushes.Length);
      return predefinedBrushes[i];
    }

    protected override LoggedFlight ConvertBack(Brush value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
