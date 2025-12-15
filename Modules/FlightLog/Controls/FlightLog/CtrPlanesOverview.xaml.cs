using Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog.Stats;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling;
using ESystem.Miscelaneous;
using Mapsui.Tiling;
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

    //if (stats.FlightConnections.Count > 0)
    //{
    //  var firstIcao = stats.FlightConnections[0].Item1;
    //  var firstLocation = AirportInfoProvider.Instance.GetAirportInfo(firstIcao)?.Location;
    //  if (firstLocation != null)
    //  {
    //    var firstPoint = new Mapsui.Geometries.Point(firstLocation.Value.Longitude, firstLocation.Value.Latitude);
    //    var firstSphericalMercator = Mapsui.Projections.SphericalMercator.FromLonLat(firstPoint.X, firstPoint.Y);
    //    map.Home = n => n.NavigateTo(firstSphericalMercator, map.Resolutions[12]);
    //  }
    //  var lineString = new Mapsui.Geometries.LineString();
    //  foreach (var connection in stats.FlightConnections)
    //  {
    //    var fromIcao = connection.Item1;
    //    var toIcao = connection.Item2;
    //    var fromLocation = AirportInfoProvider.Instance.GetAirportInfo(fromIcao)?.Location;
    //    var toLocation = AirportInfoProvider.Instance.GetAirportInfo(toIcao)?.Location;
    //    if (fromLocation != null && toLocation != null)
    //    {
    //      var fromPoint = new Mapsui.Geometries.Point(fromLocation.Value.Longitude, fromLocation.Value.Latitude);
    //      var toPoint = new Mapsui.Geometries.Point(toLocation.Value.Longitude, toLocation.Value.Latitude);
    //      var fromSphericalMercator = Mapsui.Projections.SphericalMercator.FromLonLat(fromPoint.X, fromPoint.Y);
    //      var toSphericalMercator = Mapsui.Projections.SphericalMercator.FromLonLat(toPoint.X, toPoint.Y);
    //      lineString.Vertices.Add(fromSphericalMercator);
    //      lineString.Vertices.Add(toSphericalMercator);
    //    }
    //  }
    //  var lineFeature = new Mapsui.Features.Feature { Geometry = lineString };
    //  var lineStyle = new Mapsui.Styles.VectorStyle
    //  {
    //    Line = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Red, 2)
    //  };
    //  lineFeature.Styles.Add(lineStyle);
    //  var layer = new Mapsui.Layers.MemoryLayer
    //  {
    //    Name = "Flight Connections",
    //    Features = new Mapsui.Collections.Features { lineFeature },
    //    Style = null
    //  };
    //  map.Layers.Add(layer);
    //}
  }
}
