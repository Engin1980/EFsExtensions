using Eng.EFsExtensions.Modules.FlightLogModule.Controls.Shared;
using Eng.EFsExtensions.Modules.FlightLogModule.Converters;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using ESystem;
using ESystem.Structs;
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

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  /// <summary>
  /// Interaction logic for LogStats.xaml
  /// </summary>
  public partial class CtrLogStats : FlightsBasedUserControl
  {
    public record StatsData(OverallStats OverallStats, List<DescriptiveLogStatView> DescriptiveStats, List<GroupingLogStatView> GroupingStats, List<LoggedFlight> Flights);
    public CtrLogStats()
    {
      InitializeComponent();
      this.FlightsChanged += () =>
      {
        this.ctrFilter.SetUpFilter(this.Flights);
        ReevalStats();
      };
      this.ctrFilter.FilterChanged += ReevalStats;
      this.pnlMain.DataContext = null; // force to ignore DataContext set by parent
    }

    private void ReevalStats()
    {
      var flights = this.Flights;
      var filteredFlights = this.ctrFilter.ApplyFilter(flights);
      var filteredStats = Calculate(filteredFlights);
      this.pnlMain.DataContext = filteredStats;
    }

    private static StatsData Calculate(List<LoggedFlight> flights)
    {
      if (flights == null) return new StatsData(new OverallStats(0, TimeSpan.Zero, TimeSpan.Zero), [], [], []);

      OverallStats all = new(flights.Count, TimeSpan.FromTicks(flights.Sum(q => q.BlockTime.Ticks)), TimeSpan.FromTicks(flights.Sum(q => q.AirTime.Ticks)));
      List<DescriptiveLogStatView> des = [];
      List<GroupingLogStatView> grp = [];

      foreach (DescriptiveLogStatItem stat in Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel.LogStats.DescriptiveLogStats)
      {
        var tmp = CalculateStat(stat, flights);
        if (tmp is not null)
          des.Add(tmp);
      }

      foreach (GroupingLogStatItem stat in Models.LogModel.LogStats.GroupingLogStats)
      {
        var tmp = CalculateStat(stat, flights);
        if (tmp is not null)
          grp.Add(tmp);
      }

      StatsData ret = new(all, des, grp, flights);

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

      GroupingLogStatView view = new(stat, records, uniqueCount);

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
        if (stat.ValueConverter is not null)
        {
          object input = stat.ValueConverter is LongDistanceConverter || stat.ValueConverter is ShortDistanceConverter
            ? new Distance(value, DistanceUnit.Meters)
            : stat.ValueConverter is WeightConverter
            ? new Weight(value, WeightUnit.Kilograms)
            : stat.ValueConverter is SpeedConverter
            ? new Speed(value, SpeedUnit.KTS)
            : throw new ApplicationException($"Unexepected converter type '{stat.ValueConverter.GetType()}'");
          return (string)stat.ValueConverter.Convert(input, typeof(string), stat.ValueStringFormat, System.Globalization.CultureInfo.DefaultThreadCurrentUICulture);
        }
        else if (stat.ValueStringFormat is not null)
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
  }
}
