using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Eng.Chlaot.Modules.FlightLogModule.Navdata;
using Eng.Chlaot.Modules.FlightLogModule;
using ESystem;
using ESystem.Asserting;

namespace Eng.Chlaot.Modules.FlightLogModule.LogModel
{
  internal static class Convertor
  {
    public static Flight Convert(RunModel run, IEnumerable<Airport> airports)
    {
      EAssert.IsNotNull(run.StartUpCache);
      EAssert.IsNotNull(run.TakeOffCache);
      EAssert.IsNotNull(run.ShutDownCache);
      EAssert.IsNotNull(run.LandingCache);

      Airport? departureAirport = GetClosestAirport(airports, new GPS(run.TakeOffCache.Latittude, run.TakeOffCache.Longitude), 5);
      Airport? destinationAirport = GetClosestAirport(airports, new GPS(run.TakeOffCache.Latittude, run.TakeOffCache.Longitude), 5);

      Flight ret = new(
        departureAirport?.ICAO,
        destinationAirport?.ICAO,
        new StartUp(run.StartUpCache.Time),
        new TakeOff(run.TakeOffCache.Time),
        new Landing(run.LandingCache.Time),
        new ShutDown(run.ShutDownCache.Time));

      return ret;
    }

    private static Airport? GetClosestAirport(IEnumerable<Airport> airports, GPS gps, int maxDistanceInNm)
    {
      double minLat = gps.Latitude - 1;
      double maxLat = gps.Latitude + 1;
      double minLong = gps.Longitude - 1;
      double maxLong = gps.Longitude + 1;

      airports = airports.Where(q => IsBetween(q.Coordinate.Latitude, minLat, maxLat) && IsBetween(q.Coordinate.Longitude, minLong, maxLong));
      if (!airports.Any()) return null;

      Airport closestAirport = airports.MinBy(q => GetSimpleSquareDistance(q.Coordinate, gps)) ?? throw new ESystem.Exceptions.UnexpectedNullException();
      if (CalculateDistanceInNauticalMiles(closestAirport.Coordinate, gps) > maxDistanceInNm) return null;

      return closestAirport;
    }

    private static bool IsBetween(double value, double min, double max) => min < value && value < max;
    private static double GetSimpleSquareDistance(GPS a, GPS b) => Math.Pow(a.Latitude - b.Latitude, 2) + Math.Pow(a.Longitude - b.Longitude, 2);
    private static double CalculateDistanceInNauticalMiles(GPS a, GPS b) => CalculateDistanceInNauticalMiles(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
    private static double CalculateDistanceInNauticalMiles(double lat1, double lon1, double lat2, double lon2)
    {
      double ToRadians(double angleInDegrees)
      {
        return angleInDegrees * (Math.PI / 180.0);
      };

      const double R = 6371; // Radius of the Earth in kilometers
      double dLat = ToRadians(lat2 - lat1);
      double dLon = ToRadians(lon2 - lon1);
      double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                 Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                 Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
      double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
      double distanceInKm = R * c;
      double distanceInNauticalMiles = distanceInKm * 0.539957; // Conversion factor
      return distanceInNauticalMiles;
    }


  }
}
