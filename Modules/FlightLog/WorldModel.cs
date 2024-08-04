using ESystem.Asserting;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FlightLogModule.Navdata
{

  public record struct GPS(double Latitude, double Longitude);

  public record RunwayThreshold(string Designator, GPS Coordinate)
  {
    public Runway Runway { get; set; } = null!;
  }

  public class Runway
  {
    public RunwayThreshold ThresholdA { get; set; }
    public RunwayThreshold ThresholdB { get; set; }
    public Airport Airport { get; set; } = null!;
    public IEnumerable<RunwayThreshold> Thresholds { get; private set; }
    public string Designator { get; private set; }

    public Runway(RunwayThreshold thresholdA, RunwayThreshold thresholdB)
    {
      this.ThresholdA = thresholdA;
      this.ThresholdB = thresholdB;
      this.Thresholds = new List<RunwayThreshold> { this.ThresholdA, this.ThresholdB };
      this.Designator = string.Join("-", this.Thresholds.Select(q => q.Designator).OrderBy(q => q));
    }

  }

  public record Airport(string ICAO, string Name, string CountryCode, string City, GPS Coordinate)
  {
    public List<Runway> Runways { get; set; } = null!;
  }

  public static class Loader
  {
    public static Dictionary<string, Airport> LoadAirports(string csvFileName)
    {
      string[][] data = ReadCsvFile(csvFileName, true);
      Dictionary<string, Airport> ret = DecodeAirports(data);
      return ret;
    }

    public static void LoadRunways(string csvFileName, Dictionary<string, Airport> airports)
    {
      string[][] data = ReadCsvFile(csvFileName, true);
      DecodeRunways(data, airports);
    }

    private static void DecodeRunways(string[][] data, Dictionary<string, Airport> airports)
    {
      const int IDX_ICAO = 2;
      const int IDX_CLOSED = 7;
      const int IDX_A_ID = 8;
      const int IDX_A_LATITUDE = 9;
      const int IDX_A_LONGITUDE = 10;
      //const int IDX_A_ELEVATION = 11;
      //const int IDX_A_HEADING = 12;
      const int IDX_B_ID = 14;
      const int IDX_B_LATITUDE = 15;
      const int IDX_B_LONGITUDE = 16;
      //const int IDX_B_ELEVATION = 17;
      //const int IDX_B_HEADING = 18;

      var tmp = data.Where(q => q[IDX_CLOSED] == "0");
      tmp = tmp.Where(q => !string.IsNullOrEmpty(q[IDX_A_LATITUDE]) && !string.IsNullOrEmpty(q[IDX_A_LONGITUDE]));
      tmp = tmp.Where(q => !string.IsNullOrEmpty(q[IDX_B_LATITUDE]) && !string.IsNullOrEmpty(q[IDX_B_LONGITUDE]));
      tmp = tmp.Where(q => airports.ContainsKey(q[IDX_ICAO]));
      data = tmp.ToArray();

      foreach (var row in data)
      {
        Airport airport = airports[row[IDX_ICAO]];

        RunwayThreshold rta = new(row[IDX_A_ID], CreateGps(row[IDX_A_LATITUDE], row[IDX_A_LONGITUDE]));
        RunwayThreshold rtb = new(row[IDX_B_ID], CreateGps(row[IDX_B_LATITUDE], row[IDX_B_LONGITUDE]));
        Runway runway = new(rta, rtb);
        rta.Runway = rtb.Runway = runway;

        runway.Airport = airport;
        airport.Runways.Add(runway);
      }
    }

    private static Dictionary<string, Airport> DecodeAirports(string[][] data)
    {
      const string TYPE_MEDIUM = "medium_airport";
      const string TYPE_LARGE = "large_airport";
      const int IDX_ICAO = 1;
      const int IDX_TYPE = 2;
      const int IDX_NAME = 3;
      const int IDX_COUNTRY = 8;
      const int IDX_CITY = 10;
      const int IDX_LATITUDE = 4;
      const int IDX_LONGITUDE = 5;

      Dictionary<string, Airport> ret = new();
      var tmp = data.Where(q => q[IDX_ICAO].Length == 4);
      tmp = tmp.Where(q => !string.IsNullOrEmpty(q[IDX_LATITUDE]));
      tmp = tmp.Where(q => q[IDX_TYPE] == TYPE_MEDIUM || q[IDX_TYPE] == TYPE_LARGE);
      data = tmp.ToArray();
      // data = data.Where(q => q[IDX_ICAO].StartsWith("LK")).ToArray();
      foreach (var row in data)
      {
        Airport airport;
        try
        {
          airport = new(row[IDX_ICAO], row[IDX_NAME], row[IDX_COUNTRY], row[IDX_CITY], CreateGps(row[IDX_LATITUDE], row[IDX_LONGITUDE]));
          airport.Runways = new();
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Failed to decode airport from " + row, ex);
        }
        ret[airport.ICAO] = airport;
      }

      return ret;
    }

    private static readonly CultureInfo cultureEnUs = CultureInfo.GetCultureInfo("en-US");

    private static GPS CreateGps(string latitudeString, string longitudeString)
    {
      GPS ret;

      try
      {
        double latitude = double.Parse(latitudeString, cultureEnUs);
        double longitude = double.Parse(longitudeString, cultureEnUs);
        ret = new(latitude, longitude);
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Failed to decode GPS from {latitudeString} and {longitudeString}.", ex);
      }
      return ret;
    }

    private static string[][] ReadCsvFile(string csvFileName, bool skipHeader)
    {
      List<string[]> ret = new();

      using (TextFieldParser parser = new(csvFileName))
      {
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");

        if (skipHeader)
          parser.ReadLine();

        while (!parser.EndOfData)
        {
          string[] fields = parser.ReadFields() ?? throw new ESystem.Exceptions.UnexpectedNullException();
          ret.Add(fields);
        }
      }

      EAssert.IsTrue(ret.Select(q => q.Length).Distinct().Count() == 1, "Not all lines have the same number of elements.");

      return ret.ToArray();
    }
  }
}
