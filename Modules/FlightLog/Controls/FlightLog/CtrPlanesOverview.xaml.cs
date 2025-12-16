using BruTile.Wms;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.Stats;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling;
using ESystem.Miscelaneous;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog;

/// <summary>
/// Interaction logic for CtrPlanesOverview.xaml
/// </summary>
public partial class CtrPlanesOverview : UserControl
{
  public CtrPlanesOverview()
  {
    InitializeComponent();
  }


  private void UserControl_Loaded(object sender, RoutedEventArgs e)
  {
    //TODO
    var stats = (LogViewModel)this.DataContext;
    var map = new Mapsui.Map();
    map.Layers.Add(OpenStreetMap.CreateTileLayer());
    ctrMap.Map = map;

    var registrations = stats.Flights
      .Select(q => q.AircraftRegistration ?? string.Empty)
      .Distinct()
      .ToList();

    const double goldenAngle = 137.50776405003785;
    (byte R, byte G, byte B) convertHsvToRgb(
        double h, double s, double v)
    {
      h = h % 360;
      if (h < 0) h += 360;

      double c = v * s;
      double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
      double m = v - c;

      double r1 = 0, g1 = 0, b1 = 0;

      if (h < 60)
      {
        r1 = c; g1 = x; b1 = 0;
      }
      else if (h < 120)
      {
        r1 = x; g1 = c; b1 = 0;
      }
      else if (h < 180)
      {
        r1 = 0; g1 = c; b1 = x;
      }
      else if (h < 240)
      {
        r1 = 0; g1 = x; b1 = c;
      }
      else if (h < 300)
      {
        r1 = x; g1 = 0; b1 = c;
      }
      else
      {
        r1 = c; g1 = 0; b1 = x;
      }

      byte r = (byte)Math.Round((r1 + m) * 255);
      byte g = (byte)Math.Round((g1 + m) * 255);
      byte b = (byte)Math.Round((b1 + m) * 255);

      return (r, g, b);
    }
    Mapsui.Styles.Color getColorByRegistration(string registration)
    {
      int colorIndex = registration.GetHashCode() % 512;
      const byte a = 255;
      byte r = 255, g = 255, b = 255;
      double h = colorIndex * goldenAngle % 360;
      double s = 0.8;
      double v = 0.8;
      (r, g, b) = convertHsvToRgb(h, s, v);
      Mapsui.Styles.Color ret = Mapsui.Styles.Color.FromArgb(a, r, g, b);
      return ret;
    }

    var registrationLineStyles = registrations
      .Select((reg, index) =>
      {
        var color = getColorByRegistration(reg);
        return new
        {
          Registration = reg,
          Style = new VectorStyle
          {
            Line = new Mapsui.Styles.Pen
            {
              Color = color,
              Width = 0.5
            },
            Fill = new Mapsui.Styles.Brush(color),
            Outline = null
          }
        };
      })
      .ToDictionary(q => q.Registration, q => q.Style);


    var tmps = stats.Flights
      .Where(q => q.DepartureICAO != null && q.LandedICAO != null)
      .Select(q => q.DepartureICAO!.CompareTo(q.LandedICAO!) < 0
        ? new { A = q.DepartureICAO, B = q.LandedICAO, Flight = q }
        : new { A = q.LandedICAO, B = q.DepartureICAO, Flight = q })
      .DistinctBy(q => (q.A, q.B))
      .ToList();

    var icaos = tmps
      .SelectMany(q => new[] { q.A, q.B })
      .Distinct()
      .ToList();

    var gpsLocations = new Dictionary<string, GPS>();
    foreach (var icao in icaos)
    {
      var flight = stats.Flights.First(q => q.DepartureICAO == icao || q.LandedICAO == icao);
      GPS gps = flight.DepartureICAO == icao ? flight.TakeOffLocation : flight.Touchdowns.Last().TouchDownLocation;
      gpsLocations[icao] = gps;
    }
    var mapsuiLocations = gpsLocations.ToDictionary(
      q => q.Key,
      q => SphericalMercator.FromLonLat(gpsLocations[q.Key].Longitude, gpsLocations[q.Key].Latitude)
      );

    var trackLayer = new MemoryLayer()
    {
      Name = "Flights",
      Style = null
    };

    List<Mapsui.IFeature> features = [];

    foreach (var tmp in tmps)
    {
      var a = mapsuiLocations[tmp.A];
      var b = mapsuiLocations[tmp.B];
      var line = new LineString([a, b]);
      var feature = new Mapsui.Nts.GeometryFeature(line)
      {
        Styles = [registrationLineStyles[tmp.Flight.AircraftRegistration ?? string.Empty]]
      };
      features.Add(feature);
    }

    trackLayer.Features = features;
    map.Layers.Add(trackLayer);

    const double VIEW_BORDER = 2000000;
    var minX = mapsuiLocations.Min(q => q.Value.x) - VIEW_BORDER;
    var maxX = mapsuiLocations.Max(q => q.Value.x) + VIEW_BORDER;
    var minY = mapsuiLocations.Min(q => q.Value.y) - VIEW_BORDER;
    var maxY = mapsuiLocations.Max(q => q.Value.y) + VIEW_BORDER;
    Mapsui.MRect mrect = new(minX, minY, maxX, maxY);
    map.Navigator.ZoomToBox(mrect);
  }
}
