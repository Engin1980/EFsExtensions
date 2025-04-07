using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel
{
  internal class LogStats
  {
    #region static
    public static List<DescriptiveLogStatItem> DescriptiveLogStats { get; set; } = new List<DescriptiveLogStatItem>();
    public static List<GroupingLogStatItem> GroupingLogStats { get; set; } = new List<GroupingLogStatItem>();

    static LogStats()
    {
      DescriptiveLogStats.Add(new("Flight Time", q => q.AirTime.Ticks, ValueStringFormatter: q => new TimeSpan((long)q).ToString(@"h\:mm\:ss")));
      DescriptiveLogStats.Add(new("Block Time", q => q.BlockTime.Ticks, ValueStringFormatter: q => new TimeSpan((long)q).ToString(@"h\:mm\:ss")));
      DescriptiveLogStats.Add(new("Air Time Ratio", q => q.AirTime.TotalSeconds * 100 / q.BlockTime.TotalSeconds, "{0:N1} %"));

      DescriptiveLogStats.Add(new("Distance", q => q.Distance, "{0:N0} NM"));
      DescriptiveLogStats.Add(new("Fuel Used", q => q.StartUpFuelWeight - q.ShutDownFuelWeight, "{0:N0} kg"));
      DescriptiveLogStats.Add(new("Landing Fuel", q => q.LandingFuelWeight, "{0:N0} kg"));

      DescriptiveLogStats.Add(new("Takeoff IAS", q => q.TakeOff.IAS, "{0:N0} kt"));
      DescriptiveLogStats.Add(new("Takeoff VS", q => q.TakeOff.MaxVS, "{0:N0} ft/min"));
      DescriptiveLogStats.Add(new("Takeoff Bank", q => q.TakeOff.MaxBank, "{0:N0}°"));
      DescriptiveLogStats.Add(new("Takeoff Pitch", q => q.TakeOff.MaxPitch, "{0:N0}°"));
      DescriptiveLogStats.Add(new("Takeoff MaxAccY", q => q.TakeOff.MaxAccY, "{0:N0}"));
      DescriptiveLogStats.Add(new("Takeoff Run", q => q.TakeOff.Length, "{0:N3} NM"));

      DescriptiveLogStats.Add(new("Landing VS", q => q.Touchdowns.Last().VS, "{0:N3} ft/min"));
      DescriptiveLogStats.Add(new("Landing IAS", q => q.Touchdowns.Last().IAS, "{0:N0} kt"));
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
    }

    public static StatsData Calculate(List<LoggedFlight> flights)
    {
      OverallStats all = new(flights.Count, TimeSpan.FromTicks(flights.Sum(q => q.BlockTime.Ticks)), TimeSpan.FromTicks(flights.Sum(q => q.AirTime.Ticks)));
      List<DescriptiveLogStatView> des = new();
      List<GroupingLogStatView> grp = new();

      foreach (DescriptiveLogStatItem stat in DescriptiveLogStats)
      {
        var tmp = CalculateStat(stat, flights);
        if (tmp is not null)
          des.Add(tmp);
      }

      foreach (GroupingLogStatItem stat in GroupingLogStats)
      {
        var tmp = CalculateStat(stat, flights);
        if (tmp is not null)
          grp.Add(tmp);
      }

      StatsData ret = new(all, des, grp);

      return ret;
    }

    private static GroupingLogStatView? CalculateStat(GroupingLogStatItem stat, List<LoggedFlight> flights)
    {
      var tmp = flights
        .Where(q => stat.GroupSelector(q) != null);

      if (!tmp.Any()) return null;

      int uniqueCount = -1;

      List<GroupingLogStatRecord> records = tmp
        .GroupBy(stat.GroupSelector)
        .Tap(q => uniqueCount = q.Count())
        .Select(q => new GroupingLogStatRecord(q.Count(), q.Key!, q.ToList()))
        .OrderByDescending(q => q.Count)
        .ToList();

      GroupingLogStatView view = new GroupingLogStatView(stat, records, uniqueCount);

      return view;
    }

    private static DescriptiveLogStatView? CalculateStat(DescriptiveLogStatItem stat, List<LoggedFlight> flights)
    {
      var tmp = flights.Select(q => new { Value = stat.ValueSelector(q), Flight = q });
      tmp = tmp.Where(q => q.Value.HasValue);

      if (!tmp.Any()) return null;

      var min = tmp.MinBy(q => q.Value!.Value);
      var max = tmp.MaxBy(q => q.Value!.Value);
      var avg = tmp.Average(q => q.Value!.Value);

      string formatByStats(double value, DescriptiveLogStatItem stat)
      {
        if (stat.ValueStringFormat is not null)
          return string.Format(stat.ValueStringFormat, value);
        else if (stat.ValueStringFormatter is not null)
          return stat.ValueStringFormatter(value);
        else
          return value.ToString();
      }

      DescriptiveLogStatRecord createStats(double value, LoggedFlight flight)
      {
        return new(value, formatByStats(value, stat), flight);
      }

      DescriptiveLogStatView view = new(
        stat,
        createStats(min!.Value!.Value, min.Flight),
        createStats(max!.Value!.Value, max.Flight),
        formatByStats(avg, stat));

      return view;
    }
    #endregion static

    private LogStats() { }
  }

  public record StatsData(OverallStats OverallStats, List<DescriptiveLogStatView> DescriptiveStats, List<GroupingLogStatView> GroupingStats);

  public record OverallStats(int TotalFlights, TimeSpan TotalBlockDuration, TimeSpan TotalAirDuration);

  public record GroupingLogStatItem(string Title, Func<LoggedFlight, object?> GroupSelector);
  public record GroupingLogStatRecord(int Count, object Key, List<LoggedFlight> Flights);
  public record GroupingLogStatView(
    GroupingLogStatItem Stat,
    List<GroupingLogStatRecord> Records,
    int UniqueCount)
  {
    public GroupingLogStatRecord First => Records.First();
    public GroupingLogStatRecord? Second => Records.Count() > 1 ? Records.Skip(1).First() : null;
    public GroupingLogStatRecord? Third => Records.Count() > 2 ? Records.Skip(2).First() : null;
    public GroupingLogStatRecord? Last => Records.Count() > 3 ? Records.Last() : null;
  }

  public record DescriptiveLogStatItem(string Title, Func<LoggedFlight, double?> ValueSelector,
    string? ValueStringFormat = null, Func<double, string>? ValueStringFormatter = null);
  public record DescriptiveLogStatRecord(double Value, string DisplayValue, LoggedFlight Flight);
  public record DescriptiveLogStatView(
    DescriptiveLogStatItem Stat,
    DescriptiveLogStatRecord Min,
    DescriptiveLogStatRecord Max,
    string Avg)
  {
  }
}
