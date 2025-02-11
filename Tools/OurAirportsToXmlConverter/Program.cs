// See https://aka.ms/new-console-template for more information

using OurAirportsToXmlConverter;

if (!System.IO.File.Exists("airports.csv")) throw new ApplicationException("File 'airports.csv' not found.");
if (!System.IO.File.Exists("runways.csv")) throw new ApplicationException("File 'runways.csv' not found.");

Console.WriteLine("Loading airports");
var airports = CsvLoader.LoadAirports("airports.csv");

Console.WriteLine("Loading runways");
CsvLoader.LoadRunways("runways.csv", airports);

airports = airports.Where(q => q.Runways.Count > 0).ToList();
airports.ForEach(q=>q.Declination = GetApproximateDeclination(q.Coordinate.Latitude, q.Coordinate.Longitude));
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