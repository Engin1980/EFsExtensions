using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using ESystem;
using ESystem.Miscelaneous;
using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
using System.Windows.Shapes;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.Stats.DescriptiveStats
{
  /// <summary>
  /// Interaction logic for DescriptiveStatsHistogram.xaml
  /// </summary>
  public partial class DescriptiveStatsHistogram : Window
  {
    private double[] values = [];
    private CtrLogStats.StatsData statsData = null!;
    private DescriptiveLogStatView stats = null!;

    private record FlightStatView(
      int Index,
      string Callsign,
      string Registration,
      string From,
      string To,
      DateTime Date,
      double Value,
      LoggedFlight Flight);

    private class PageViewModel : NotifyPropertyChanged
    {
      public int NumberHistogramOfBins
      {
        get => base.GetProperty<int>(nameof(NumberHistogramOfBins));
        set => base.UpdateProperty(nameof(NumberHistogramOfBins), value);
      }

      public List<FlightStatView> Data
      {
        get => base.GetProperty<List<FlightStatView>>(nameof(Data))!;
        set => base.UpdateProperty(nameof(Data), value);
      }

      public PageViewModel()
      {
        this.NumberHistogramOfBins = 20;
        this.Data = [];
      }
    }
    private readonly PageViewModel viewModel;

    public DescriptiveStatsHistogram()
    {
      InitializeComponent();
      this.DataContext = viewModel = new();
      this.viewModel.PropertyChanged += (s, e) =>
      {
        if (values == null || values.Length == 0) return;
        RebuildHistogram();
        RebuildScatterPlot();
      };
    }

    private void RebuildScatterPlot()
    {
      var scatterPoints = values
        .Select((val, index) => new { val, index })
        .Select(q => new ScatterPoint(q.index, q.val))
        .ToList();

      var plotModel = new PlotModel { Title = stats.Stat.Title };

      plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "Flight Index",
        Minimum = -0.5, // Malý okraj, aby první bod nebyl na hraně
      });

      plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "Value",
      });

      var scatterSeries = new ScatterSeries
      {
        MarkerType = MarkerType.Circle,
        MarkerSize = 4,
        MarkerFill = OxyColors.DodgerBlue,
        ItemsSource = scatterPoints
      };

      plotModel.Series.Add(scatterSeries);
      pltScatterPlot.Model = plotModel;
    }

    private void RebuildHistogram()
    {
      var binningOptions = new BinningOptions(
        BinningOutlierMode.CountOutliers,
        BinningIntervalType.InclusiveLowerBound | BinningIntervalType.InclusiveUpperBound,
        BinningExtremeValueMode.IncludeExtremeValues);
      var bins = HistogramHelpers.CreateUniformBins(values.Min(), values.Max(), this.viewModel.NumberHistogramOfBins);
      var histogram = HistogramHelpers.Collect(values, bins, binningOptions);

      foreach (var item in histogram)
      {
        item.Area = item.Count * item.Width;
      }

      var histogramModel = new PlotModel { Title = stats.Stat.Title };

      histogramModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "Value"
      });

      histogramModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "Count"
      });

      histogramModel.Series.Add(new HistogramSeries
      {
        ItemsSource = histogram,
        //LabelFormatString = "{3:N2}", // Volitelné: zobrazí počet (Count) přímo nad sloupce
        //LabelPlacement = LabelPlacement.Middle,
        StrokeColor = OxyColors.Black,
        StrokeThickness = 1,
        FillColor = OxyColors.SkyBlue,
        LabelMargin = 5
      });

      pltHistogram.Model = histogramModel;
    }

    internal void SetData(CtrLogStats.StatsData statsData, DescriptiveLogStatView stats)
    {
      this.statsData = statsData;
      this.stats = stats;
      this.values = statsData.Flights
        .Select(q => stats.Stat.ValueSelector(q))
        .Where(q => q.HasValue)
        .Select(q => q!.Value)
        .ToArray();

      this.viewModel.Data = statsData.Flights
        .Where(q => stats.Stat.ValueSelector(q) != null)
        .Select((q, i) => new FlightStatView(
          i + 1,
          q.Callsign,
          q.AircraftRegistration ?? "-",
          q.DepartureICAO ?? "?",
          q.DestinationICAO ?? "?",
          q.StartUpScheduledDateTime ?? q.StartUpDateTime,
          (double)stats.Stat.ValueSelector(q)!,
          q))
        .ToList();

      RebuildHistogram();
      RebuildScatterPlot();
    }
  }
}
