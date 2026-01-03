using Eng.EFsExtensions.Modules.FlightLogModule.Controls.Shared;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESystem.Miscelaneous;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using static Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.CtrLogFlightMain;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  /// <summary>
  /// Interaction logic for CtrAirportStats.xaml
  /// </summary>
  public partial class CtrAirportStats : FlightsBasedUserControl
  {
    public class CtrAirportStatsViewModel : NotifyPropertyChanged
    {
      public List<string> ICAOs
      {
        get => base.GetProperty<List<string>>(nameof(ICAOs)) ?? [];
        set => base.UpdateProperty(nameof(ICAOs), value);
      }

      public string? SelectedICAO
      {
        get => base.GetProperty<string?>(nameof(SelectedICAO));
        set => base.UpdateProperty(nameof(SelectedICAO), value);
      }

      public Dictionary<string, List<LoggedFlight>> IcaoArrivalFlights { get; set; } = [];
      public Dictionary<string, List<LoggedFlight>> IcaoDepartureFlights { get; set; } = [];
    }
    private readonly CtrAirportStatsViewModel vm;
    public CtrAirportStats()
    {
      InitializeComponent();
      this.pnlMain.DataContext = vm = new();
      this.vm.PropertyChanged += RedrawMapIfRequired;
      this.FlightsChanged += () =>
      {
        ReevalStats();
      };
      InitMap();
      ReevalStats();
    }

    private void RedrawMapIfRequired(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName != nameof(CtrAirportStatsViewModel.SelectedICAO))
        return;

      RedrawMap();
    }

    private void ReevalStats()
    {
      if (this.Flights == null || this.Flights.Any() == false) return;

      List<LoggedFlight> flights = this.Flights;
      var icaoFlights = flights.GroupBy(q => q.DestinationICAO ?? "UNKNOWN")
        .ToDictionary(q => q.Key, q => q.ToList());
      vm.IcaoArrivalFlights = icaoFlights;
      icaoFlights = flights.GroupBy(q => q.DepartureICAO ?? "UNKNOWN")
        .ToDictionary(q => q.Key, q => q.ToList());
      vm.IcaoDepartureFlights = icaoFlights;
      vm.ICAOs = vm.IcaoDepartureFlights.Keys.Union(vm.IcaoArrivalFlights.Keys).OrderBy(q => q).ToList();

      RedrawMap();
    }

    private void InitMap()
    {
      var map = new Mapsui.Map();
      map.Layers.Add(OpenStreetMap.CreateTileLayer());
      ctrMap.Map = map;
    }

    private void RedrawMap()
    {
      //if (ctrMap.Map.Layers.Count > 1)
      //  ctrMap.Map.Layers.Remove(ctrMap.Map.Layers[1]);

      var tmp = this.vm.SelectedICAO ?? "";
      if (this.vm.IcaoArrivalFlights.ContainsKey(tmp) == false) return;
      var arrivalFlights = this.vm.IcaoArrivalFlights.ContainsKey(tmp) ? this.vm.IcaoArrivalFlights[tmp] : [];
      var departureFlights = this.vm.IcaoDepartureFlights.ContainsKey(tmp) ? this.vm.IcaoDepartureFlights[tmp] : [];

      var featuresLanding = arrivalFlights.Select(q => CreatePinForFlightLanding(q));
      var featuresLandingLines = arrivalFlights
        .Where(q => q.LastTouchdown.RollOutEndLocation != null)
        .Select(q => CreateLineForFlightLanding(q));

      var featuresTakeOff = departureFlights.Select(q => CreatePinForFlightTakeOff(q));
      var featuresTakeOffLines = departureFlights
        .Where(q => q.LastTouchdown.RollOutEndLocation != null)
        .Select(q => CreateLineForFlightTakeOff(q));

      var layer = new MemoryLayer
      {
        Name = "Landing Pins",
        Features = [.. featuresLanding, .. featuresLandingLines, .. featuresTakeOff, .. featuresTakeOffLines],
        Style = null // Styl máme definovaný přímo u feature
      };
      ctrMap.Map.Layers.Add(layer);
    }

    private IFeature CreateLineForFlightLanding(LoggedFlight flight)
    {
      var ltd = flight.LastTouchdown;
      var pointFrom = SphericalMercator.FromLonLat(ltd.TouchDownLocation.Longitude, ltd.TouchDownLocation.Latitude);
      var pointTo = SphericalMercator.FromLonLat(ltd.RollOutEndLocation!.Value.Longitude, ltd.RollOutEndLocation!.Value.Latitude);

      var lineString = new LineString([pointFrom, pointTo]);

      var feature = new GeometryFeature() { Geometry = lineString };
      feature.Styles.Add(new VectorStyle
      {
        Line = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Blue, width: 0.5)
      });
      return feature;
    }

    private IFeature CreateLineForFlightTakeOff(LoggedFlight flight)
    {
      var tko = flight.TakeOff;
      var pointFrom = SphericalMercator.FromLonLat(tko.RunStartLocation.Longitude, tko.RunStartLocation.Latitude);
      var pointTo = SphericalMercator.FromLonLat(tko.AirborneLocation.Longitude, tko.AirborneLocation.Latitude);

      var lineString = new LineString([pointFrom, pointTo]);

      var feature = new GeometryFeature() { Geometry = lineString };
      feature.Styles.Add(new VectorStyle
      {
        Line = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Green, width: 0.5)
      });
      return feature;
    }

    private IFeature CreatePinForFlightLanding(LoggedFlight flight)
    {
      var point = SphericalMercator.FromLonLat(flight.LandingLocation.Longitude, flight.LandingLocation.Latitude);
      var feature = new PointFeature(point);
      feature.Styles.Add(new SymbolStyle
      {
        SymbolScale = 0.25,
        Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Blue),
        SymbolType = SymbolType.Ellipse,
      });
      return feature;
    }

    private IFeature CreatePinForFlightTakeOff(LoggedFlight flight)
    {
      var point = SphericalMercator.FromLonLat(flight.TakeOff.RunStartLocation.Longitude, flight.TakeOff.RunStartLocation.Latitude);
      var feature = new PointFeature(point);
      feature.Styles.Add(new SymbolStyle
      {
        SymbolScale = 0.25,
        Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Green),
        SymbolType = SymbolType.Ellipse,
      });
      return feature;
    }

    //public void AddPinWithLabel(double lon, double lat, string labelText)
    //{
    //  // 1. Transformace souřadnic (z GPS stupňů do Spherical Mercator, který Mapsui používá)
    //  var point = SphericalMercator.FromLonLat(lon, lat).ToMPoint();

    //  // 2. Vytvoření prvku (Feature)
    //  var feature = new PointFeature(point);

    //  // 3. Definice stylu (Pin a Label)
    //  feature.Styles.Add(new SymbolStyle
    //  {
    //    SymbolScale = 0.5,
    //    Fill = new Brush(Color.Red),      // Barva pinu
    //    SymbolType = SymbolType.Ellipse,  // Tvar pinu
    //    Text = labelText,                 // TEXT POPISKU
    //    ForeColor = Color.Black,          // Barva textu
    //    BackColor = new Brush(Color.White), // Pozadí pod textem
    //    VerticalAlignment = VerticalAlignment.Bottom,
    //    Offset = new Offset(0, 20)        // Posun textu nad pin
    //  });

    //  // 4. Vytvoření vrstvy pro piny
    //  var layer = new MemoryLayer
    //  {
    //    Name = "Moje Piny",
    //    Features = new List<PointFeature> { feature },
    //    Style = null // Styl máme definovaný přímo u feature
    //  };

    //  // 5. Přidání do mapy
    //  MyMap.Map.Layers.Add(layer);
    //}
  }
}
