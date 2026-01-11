using Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog;
using Eng.EFsExtensions.Modules.FlightLogModule.Converters;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem;
using ESystem.Miscelaneous;
using ESystem.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel
{
  public static class LogStats
  {
    #region static
    public static List<DescriptiveLogStatItem> DescriptiveLogStats { get; set; } = new List<DescriptiveLogStatItem>();
    public static List<GroupingLogStatItem> GroupingLogStats { get; set; } = new List<GroupingLogStatItem>();

    static LogStats()
    {
      DescriptiveLogStats.Add(new("Flight Time", q => q.AirTime.TotalHours, ValueStringFormatter: q => TimeSpan.FromHours(q).ToString(@"h\:mm\:ss")));
      DescriptiveLogStats.Add(new("Block Time", q => q.BlockTime.TotalHours, ValueStringFormatter: q => TimeSpan.FromHours(q).ToString(@"h\:mm\:ss")));
      DescriptiveLogStats.Add(new("Air Time Ratio", q => q.AirTime.TotalSeconds / q.BlockTime.TotalSeconds, "{0:P1}"));

      DescriptiveLogStats.Add(new("Distance",
        q => q.Distance.To(DistanceUnit.Meters),
        ValueConverter: new LongDistanceConverter(),
        ValueStringFormat: "N0"));
      DescriptiveLogStats.Add(new("Fuel Used",
        q => (q.StartUpFuelWeight - q.ShutDownFuelWeight).To(WeightUnit.Kilograms),
        ValueConverter: new WeightConverter(),
        ValueStringFormat: "N0"));
      DescriptiveLogStats.Add(new("Landing Fuel",
        q => q.LandingFuelWeight.To(WeightUnit.Kilograms),
        ValueConverter: new WeightConverter(),
        ValueStringFormat: "N0"));

      DescriptiveLogStats.Add(new("Takeoff IAS",
        static q => q.TakeOff.IAS.To(targetUnit: SpeedUnit.KTS),
        ValueConverter: new SpeedConverter(),
        ValueStringFormat: "N0"));
      DescriptiveLogStats.Add(new("Takeoff VS", q => q.TakeOff.MaxVS, "{0:N0} ft/min"));
      DescriptiveLogStats.Add(new("Takeoff Bank", q => q.TakeOff.MaxBank, "{0:N3}°"));
      DescriptiveLogStats.Add(new("Takeoff Pitch", q => q.TakeOff.MaxPitch, "{0:N3}°"));
      DescriptiveLogStats.Add(new("Takeoff MaxAccY", q => q.TakeOff.MaxAccY, "{0:N3}"));
      DescriptiveLogStats.Add(new("Takeoff Run",
        q => q.TakeOff.Length.To(DistanceUnit.Meters),
        ValueConverter: new ShortDistanceConverter(),
        ValueStringFormat: "N0"));

      DescriptiveLogStats.Add(new("Landing VS", q => q.Touchdowns.Last().VS, "{0:N3} ft/min"));
      DescriptiveLogStats.Add(new("Landing Smart-VS", q => q.Touchdowns.Last().SmartVS, "{0:N3} ft/min"));
      DescriptiveLogStats.Add(new("Landing IAS",
        q => q.Touchdowns.Last().IAS.To(SpeedUnit.KTS),
        ValueConverter: new SpeedConverter(),
        ValueStringFormat: "N0"));
      DescriptiveLogStats.Add(new("Landing Bank", q => q.Touchdowns.Last().Bank, "{0:N3}°"));
      DescriptiveLogStats.Add(new("Landing Pitch", q => q.Touchdowns.Last().Pitch, "{0:N3}°"));
      DescriptiveLogStats.Add(new("Landing MaxAccY", q => q.Touchdowns.Last().MaxAccY, "{0:N3}"));
      //TODO landing run?


      GroupingLogStats.Add(new("Departure Airports", q => q.DepartureICAO));
      GroupingLogStats.Add(new("Arrival Airports", q => q.DestinationICAO));
      GroupingLogStats.Add(new("Flight Types", q => q.FlightRules));
      GroupingLogStats.Add(new("Registration", q => q.AircraftRegistration));
      GroupingLogStats.Add(new("Aircraft Type", q => q.AircraftType));
      GroupingLogStats.Add(new("Cruise Altitudes", q => q.CruizeAltitude));
      GroupingLogStats.Add(new("Flights/Month", q => q.StartUpScheduledDateTime?.ToString("yyyy-MM")));
      GroupingLogStats.Add(new("Flights/Year", q => q.StartUpScheduledDateTime?.ToString("yyyy")));
    }
    #endregion static
  }

  public record FleetAirplaneStats(string Registration, int TotalFlights, TimeSpan TotalTime, string LastLocationICAO, DateTime LastFlightDate);

  public record FleetStats(List<FleetAirplaneStats> Airplanes);

  public record OverallStats(int TotalFlights, TimeSpan TotalBlockDuration, TimeSpan TotalAirDuration);

  public record GroupingLogStatItem(string Title, Func<LoggedFlight, object?> GroupSelector);
  public record GroupingLogStatRecord(int Count, object Key, List<LoggedFlight> Flights);
  public record GroupingLogStatView(
    GroupingLogStatItem Stat,
    List<GroupingLogStatRecord> Records,
    int UniqueCount)
  {
    public GroupingLogStatRecord First => Records.First();
    public GroupingLogStatRecord? Second => Records.Count > 1 ? Records.Skip(1).First() : null;
    public GroupingLogStatRecord? Third => Records.Count > 2 ? Records.Skip(2).First() : null;
    public GroupingLogStatRecord? Last => Records.Count > 3 ? Records.Last() : null;
  }

  public record DescriptiveLogStatItem(string Title, Func<LoggedFlight, double?> ValueSelector,
    string? ValueStringFormat = null,
    Func<double, string>? ValueStringFormatter = null,
    IValueConverter? ValueConverter = null);
  public record DescriptiveLogStatRecord(double Value, string DisplayValue, LoggedFlight Flight);
  public record DescriptiveLogStatView(
    DescriptiveLogStatItem Stat,
    DescriptiveLogStatRecord Min,
    DescriptiveLogStatRecord Max,
    string Avg)
  {
  }
}
