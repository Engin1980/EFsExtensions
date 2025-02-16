using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurAirportsToXmlConverter
{

  public static class CsvLoader
  {
    public static List<Airport> LoadAirports(string csvFileName)
    {
      string[][] data = ReadCsvFile(csvFileName, true);
      List<Airport> ret = DecodeAirports(data);
      return ret;
    }

    public static void LoadRunways(string csvFileName, List<Airport> airports)
    {
      string[][] data = ReadCsvFile(csvFileName, true);
      DecodeRunways(data, airports);
    }

    private static int? tryParseInt(string s)
    {
      if (s == null || s.Length == 0)
        return null;
      else
        return int.Parse(s);
    }

    private static double? tryParseDouble(string s)
    {
      if (s == null || s.Length == 0)
        return null;
      else
        return double.Parse(s, cultureEnUs);
    }

    private static double parseDouble(string s)
    {
      return double.Parse(s, cultureEnUs);
    }

    private static void DecodeRunways(string[][] data, List<Airport> airports)
    {
      const int IDX_ICAO = 2;
      const int IDX_CLOSED = 7;
      const int IDX_A_ID = 8;
      const int IDX_A_LATITUDE = 9;
      const int IDX_A_LONGITUDE = 10;
      const int IDX_A_ELEVATION = 11;
      const int IDX_A_HEADING = 12;
      const int IDX_A_DISPLACED_THRESHOLD_FT = 13;
      const int IDX_B_SHIFT = 6;

      RunwayThreshold decodeRunwayTheshold(string[] row, bool isB)
      {
        try
        {
          string id = row[IDX_A_ID + (isB ? IDX_B_SHIFT : 0)];
          double latitude = parseDouble(row[IDX_A_LATITUDE + (isB ? IDX_B_SHIFT : 0)]);
          double longitude = parseDouble(row[IDX_A_LONGITUDE + (isB ? IDX_B_SHIFT : 0)]);
          GPS gps = new(latitude, longitude);
          double? elevation = tryParseDouble(row[IDX_A_ELEVATION + (isB ? IDX_B_SHIFT : 0)]);
          double? heading = tryParseDouble(row[IDX_A_HEADING + (isB ? IDX_B_SHIFT : 0)]);
          double? displacedThresholdFt = tryParseDouble(row[IDX_A_DISPLACED_THRESHOLD_FT + (isB ? IDX_B_SHIFT : 0)]);
          RunwayThreshold ret = new(row[IDX_A_ID + (isB ? IDX_B_SHIFT : 0)], gps, elevation, heading, displacedThresholdFt);
          return ret;
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Failed to decode runway threshold from " + string.Join("; ", row), ex);
        }
      };

      var tmp = data.Where(q => q[IDX_CLOSED] == "0"); // skips closed airports
      tmp = tmp.Where(q => !string.IsNullOrEmpty(q[IDX_A_LATITUDE]) && !string.IsNullOrEmpty(q[IDX_A_LONGITUDE]));
      tmp = tmp.Where(q => !string.IsNullOrEmpty(q[IDX_B_SHIFT + IDX_A_LATITUDE]) && !string.IsNullOrEmpty(q[IDX_B_SHIFT + IDX_B_SHIFT]));
      tmp = tmp.Where(q => airports.Any(a => a.ICAO == q[IDX_ICAO]));
      data = tmp.ToArray();

      foreach (var row in data)
      {
        Airport airport = airports.First(a => a.ICAO == row[IDX_ICAO]);

        List<RunwayThreshold> thresholds = [
          decodeRunwayTheshold(row, false),
          decodeRunwayTheshold(row, true)];
        thresholds.Sort((a, b) => a.Designator.CompareTo(b.Designator));

        Runway runway = new(string.Join("-", thresholds.Select(q => q.Designator).OrderBy(q => q)), thresholds);

        airport.Runways.Add(runway);
      }
    }

    private static List<Airport> DecodeAirports(string[][] data)
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

      List<Airport> ret = [];
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
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Failed to decode airport from " + row, ex);
        }
        ret.Add(airport);
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
      List<string[]> ret = [];

      using (TextFieldParser parser = new(csvFileName))
      {
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");

        if (skipHeader)
          parser.ReadLine();

        while (!parser.EndOfData)
        {
          string[] fields = parser.ReadFields() ?? throw new ApplicationException("Unexpected null when parsing CSV file. Invalid content?");
          ret.Add(fields);
        }
      }

      Trace.Assert(ret.Select(q => q.Length).Distinct().Count() == 1, "Not all lines have the same number of elements.");

      return [.. ret];
    }
  }
}
