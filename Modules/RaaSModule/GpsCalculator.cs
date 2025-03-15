using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule
{
  internal static class GpsCalculator
  {

    #region Private Fields

    private const double EarthRadiusKm = 6371.0;

    #endregion Private Fields

    // Earth's radius in kilometers

    #region Public Methods

    public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
      const double R = 6371; // Radius of the Earth in kilometers
      double dLat = ToRadians(lat2 - lat1);
      double dLon = ToRadians(lon2 - lon1);

      double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                 Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                 Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

      double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

      double distanceInM = R * c * 1000; // Distance in ometers

      return distanceInM;
    }

    public static double GetDistanceFromLine(double lat1, double lon1, double lat2, double lon2, double latP, double lonP, char unit = 'K')
    {
      double d13 = GetDistance(lat1, lon1, latP, lonP) / EarthRadiusKm; // Distance from P to A (normalized)
      double brng13 = InitialBearing(lat1, lon1, latP, lonP);
      double brng12 = InitialBearing(lat1, lon1, lat2, lon2);

      double ret = Math.Asin(Math.Sin(d13) * Math.Sin(ToRadians(brng13 - brng12))) * EarthRadiusKm;
      ret = Math.Abs(ret);
      return ret;
    }

    public static double InitialBearing(double lat1, double lon1, double lat2, double lon2)
    {
      double dLon = ToRadians(lon2 - lon1);
      double phi1 = ToRadians(lat1);
      double phi2 = ToRadians(lat2);

      double y = Math.Sin(dLon) * Math.Cos(phi2);
      double x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(dLon);

      return (ToDegrees(Math.Atan2(y, x)) + 360) % 360;
    }

    static double GetApproximateDeclination(double lat, double lon)
    {
      // Přibližné hodnoty deklinace pro různé zeměpisné oblasti (hrubý model)
      double baseDeclination = -6.0; // Přibližná deklinace pro střední šířky

      // Jednoduchá interpolace podle zeměpisné délky
      double declination = baseDeclination + (lon / 30.0); // Hrubá aproximace

      // Malá korekce podle zeměpisné šířky
      declination += (lat - 50) * 0.1; // Empirická úprava pro severní polokouli

      return declination;
    }

    #endregion Public Methods

    #region Private Methods

    private static double ToDegrees(double radians) => radians * (180 / Math.PI);

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180);

    #endregion Private Methods

  }
}
