// See https://aka.ms/new-console-template for more information

using OurAirportsToXmlConverter;

if (!System.IO.File.Exists("airports.csv")) throw new ApplicationException("File 'airports.csv' not found.");
if (!System.IO.File.Exists("runways.csv")) throw new ApplicationException("File 'runways.csv' not found.");

Console.WriteLine("Loading airports");
var airports = CsvLoader.LoadAirports("airports.csv");

Console.WriteLine("Loading runways");
CsvLoader.LoadRunways("runways.csv", airports);

airports = airports.Where(q => q.Runways.Count > 0).ToList();
Console.WriteLine($"Loaded {airports.Count} airports.");

Console.WriteLine("Saving to XML");
XmlSaver.Save(airports, "airports.xml");

Console.WriteLine("Done.");
