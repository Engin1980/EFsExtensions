// See https://aka.ms/new-console-template for more information

using OurAirportsToXmlConverter;
using System.Diagnostics;

if (!System.IO.File.Exists("airports.csv")) throw new ApplicationException("File 'airports.csv' not found.");
if (!System.IO.File.Exists("runways.csv")) throw new ApplicationException("File 'runways.csv' not found.");

Console.WriteLine("Loading airports");
var airports = CsvLoader.LoadAirports("airports.csv");

Console.WriteLine("Loading runways");
CsvLoader.LoadRunways("runways.csv", airports);

airports = airports.Where(q => q.Runways.Count > 0).ToList();
airports.ForEach(q => q.Declination = GetApproximateDeclination(q.Coordinate.Latitude, q.Coordinate.Longitude));
airports.SelectMany(q => q.Runways).ToList().ForEach(q => q.LengthInM = (int)GetRwyLength(q));
Console.WriteLine($"Loaded {airports.Count} airports.");

Console.WriteLine("Saving to XML");
XmlSaver.Save(airports, "airports.xml");

Console.WriteLine("Done.");


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
static double ToRadians(double degrees) => degrees * (Math.PI / 180);

static double GetRwyLength(Runway r)
{
  Trace.Assert(r.Thresholds.Count == 2, "Some runway does not have two thresholds.");
  RunwayThreshold t1 = r.Thresholds[0];
  RunwayThreshold t2 = r.Thresholds[1];
  var lat1 = t1.Coordinate.Latitude;
  var lon1 = t1.Coordinate.Longitude;
  var lat2 = t2.Coordinate.Latitude;
  var lon2 = t2.Coordinate.Longitude;

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