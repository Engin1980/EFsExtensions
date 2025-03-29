using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  internal class LogStats
  {
    #region static
    public static List<DescriptiveLogStatItem> DescriptiveLogStats { get; set; } = new List<DescriptiveLogStatItem>();
    public static List<GroupingLogStatItem> GroupingLogStats { get; set; } = new List<GroupingLogStatItem>();

    static LogStats()
    {
      DescriptiveLogStats.Add(new("Landing VS", q => q.Landing.Touchdowns.Last().VS, "{0:N3} ft/min"));
      DescriptiveLogStats.Add(new("Landing IAS", q => q.Landing.Touchdowns.Last().IAS, "{0:N0} kt"));
      DescriptiveLogStats.Add(new("Landing Bank", q => q.Landing.Touchdowns.Last().Bank, "{0:N3}°"));
      DescriptiveLogStats.Add(new("Landing Pitch", q => q.Landing.Touchdowns.Last().Pitch, "{0:N3}°"));
      DescriptiveLogStats.Add(new("Landing MaxAccY", q => q.Landing.Touchdowns.Last().MaxAccY, "{0:N3}"));
      DescriptiveLogStats.Add(new("Takeoff IAS", q => q.TakeOff.IAS, "{0:N0} kt"));

      DescriptiveLogStats.Add(new("Distance", q => q.FlightDistance, "{0:N0} NM"));
      DescriptiveLogStats.Add(new("Fuel Used", q => q.StartUp.FuelAmountKg - q.ShutDown.FuelAmountKg, "{0:N0} kg"));

      DescriptiveLogStats.Add(new("Flight Time", q => q.AirTime.TotalHours, "{0:N2} hrs"));
      DescriptiveLogStats.Add(new("Block Time", q => q.BlockTime.TotalHours, "{0:N2} hrs"));
      DescriptiveLogStats.Add(new("Air Time Ratio", q => q.AirTime.TotalSeconds * 100 / q.BlockTime.TotalSeconds, "{0:N1} %"));

      GroupingLogStats.Add(new("Departure Airports", q => q.DepartureICAO));
      GroupingLogStats.Add(new("Arrival Airports", q => q.DestinationICAO));
      // TODO GroupingLogStats.Add(new("Flight Types", q => q));
      GroupingLogStats.Add(new("Registration", q => q.AircraftRegistration));
      GroupingLogStats.Add(new("Aircraft Type", q => q.AircraftType));
      GroupingLogStats.Add(new("Cruise Altitudes", q => q.CruizeAltitude));
    }

    public static StatsData Calculate(List<LogFlight> flights)
    {
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

      StatsData ret = new(des, grp);

      return ret;
    }

    private static GroupingLogStatView? CalculateStat(GroupingLogStatItem stat, List<LogFlight> flights)
    {
      var tmp = flights
        .Where(q => stat.GroupSelector(q) != null);

      if (!tmp.Any()) return null;

      List<GroupingLogStatRecord> records = tmp
        .GroupBy(stat.GroupSelector)
        .Select(q => new GroupingLogStatRecord(q.Count(), q.Key!, q.ToList()))
        .OrderByDescending(q => q.Count)
        .ToList();

      GroupingLogStatView view = new GroupingLogStatView(stat, records);

      return view;
    }

    private static DescriptiveLogStatView? CalculateStat(DescriptiveLogStatItem stat, List<LogFlight> flights)
    {
      var tmp = flights.Select(q => new { Value = stat.ValueSelector(q), Flight = q });
      tmp = tmp.Where(q => q.Value.HasValue);

      if (!tmp.Any()) return null;

      var min = tmp.MinBy(q => q.Value!.Value);
      var max = tmp.MaxBy(q => q.Value!.Value);
      var avg = tmp.Average(q => q.Value!.Value);


      DescriptiveLogStatRecord createStats(double value, LogFlight flight) => new(value, String.Format(stat.StringFormat, value), flight);

      DescriptiveLogStatView view = new(
        stat,
        createStats(min!.Value!.Value, min.Flight),
        createStats(max!.Value!.Value, max.Flight),
        avg);


      return view;
    }
    #endregion static

    private LogStats() { }
  }

  public record StatsData(List<DescriptiveLogStatView> DescriptiveStats, List<GroupingLogStatView> GroupingStats);
  public record GroupingLogStatItem(string Title, Func<LogFlight, object?> GroupSelector);
  public record GroupingLogStatRecord(int Count, object Key, List<LogFlight> Flights);
  public record GroupingLogStatView(
    GroupingLogStatItem Stat,
    List<GroupingLogStatRecord> Records)
  {
    public GroupingLogStatRecord Best => Records.First();
  }

  public record DescriptiveLogStatItem(string Title, Func<LogFlight, double?> ValueSelector, string StringFormat = "{0:N3}");
  public record DescriptiveLogStatRecord(double Value, string DisplayValue, LogFlight Flight);
  public record DescriptiveLogStatView(
    DescriptiveLogStatItem Stat,
    DescriptiveLogStatRecord Min,
    DescriptiveLogStatRecord Max,
    double Avg)
  {
  }
}
