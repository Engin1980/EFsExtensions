using ESystem.Asserting;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Libs.AirportsLib
{
  public class AirportList : List<Airport>
  {
    public AirportList()
    {
    }

    public AirportList(IEnumerable<Airport> collection) : base(collection)
    {
    }

    public class NearestAirportOptions
    {
      public double MaxDistanceInKm = 8;
    }

    public Airport this[string icao]
    {
      get => this.First(a => a.ICAO == icao);
    }

    public Airport GetNearestAirport(GPS gps, Action<NearestAirportOptions>? opt = null) => GetNearestAirport(gps.Latitude, gps.Longitude, opt);

    public Airport GetNearestAirport(double latitude, double longitude, Action<NearestAirportOptions>? opt = null)
    {
      var ret = TryGetNearestAirport(latitude, longitude, opt);
      return ret ?? throw new Exception($"No nearest airport found for coordinates: {latitude}, {longitude}");
    }

    public Airport? TryGetNearestAirport(GPS gps, Action<NearestAirportOptions>? opt = null)
      => TryGetNearestAirport(gps.Latitude, gps.Longitude, opt);

    public Airport? TryGetNearestAirport(double latitude, double longitude, Action<NearestAirportOptions>? opt = null)
    {
      NearestAirportOptions opts = new();
      opt?.Invoke(opts);


      var closeAirports = this
        .Where(q => q.Coordinate.Latitude > latitude - 1)
        .Where(q => q.Coordinate.Latitude < latitude + 1)
        .Where(q => q.Coordinate.Longitude > longitude - 1)
        .Where(q => q.Coordinate.Longitude < longitude + 1);

      if (!closeAirports.Any())
      {
        closeAirports = this.Where(q => q.Coordinate.Latitude > latitude - 5)
          .Where(q => q.Coordinate.Latitude < latitude + 5)
          .Where(q => q.Coordinate.Longitude > longitude - 5)
          .Where(q => q.Coordinate.Longitude < longitude + 5);

        if (!closeAirports.Any())
        {
          closeAirports = this.Where(q => q.Coordinate.Latitude > latitude - 20)
            .Where(q => q.Coordinate.Latitude < latitude + 20)
            .Where(q => q.Coordinate.Longitude > longitude - 20)
            .Where(q => q.Coordinate.Longitude < longitude + 20);

          if (!closeAirports.Any())
          {
            closeAirports = this;
          }
        }
      }

      var tmpAD = closeAirports
        .Select(q => new
        {
          Airport = q,
          Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, latitude, longitude)
        })
        .MinBy(q => q.Distance) ?? throw new UnexpectedNullException();

      EAssert.IsNotNull(tmpAD, "tmpAD");
      EAssert.IsNotNull(tmpAD.Airport, "tmpAD.Airport");

      Airport? ret = tmpAD.Distance < opts.MaxDistanceInKm ? tmpAD.Airport : null;
      return ret;
    }
  }
}
