using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OurAirportsToXmlConverter
{
  public struct GPS
  {
    [XmlAttribute]
    public double Latitude { get; set; }
    [XmlAttribute]
    public double Longitude { get; set; }

    public GPS(double latitude, double longitude)
    {
      Latitude = latitude;
      Longitude = longitude;
    }
    public GPS()
    {
      Latitude = double.NaN;
      Longitude = double.NaN;
    }
  }
  public class RunwayThreshold
  {
    [XmlAttribute]
    public string Designator { get; set; } = "";
    public GPS Coordinate { get; set; } = new GPS();
    public double? Elevation { get; set; } = null;
    public double? Heading { get; set; } = null;
    public double? DisplacedThresholdFt { get; set; } = null;

    public RunwayThreshold()
    {
    }

    public RunwayThreshold(string designator, GPS coordinate, double? elevation, double? heading, double? displacedThresholdFt)
    {
      Designator = designator;
      Coordinate = coordinate;
      Elevation = elevation;
      Heading = heading;
      DisplacedThresholdFt = displacedThresholdFt;
    }
  }
  public class Runway
  {
    [XmlAttribute]
    public string Designator { get; set; } = "";
    public List<RunwayThreshold> Thresholds { get; set; } = [];
    [XmlAttribute]
    public int LengthInM { get; set; }

    public Runway(string designator, List<RunwayThreshold> thresholds)
    {
      Designator = designator;
      Thresholds = thresholds;
    }

    public Runway()
    {
    }
  }
  public class Airport
  {
    [XmlAttribute]
    public string ICAO { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountryCode { get; set; } = "";
    public string City { get; set; } = "";
    public GPS Coordinate { get; set; }
    public double Declination { get; set; }
    public List<Runway> Runways { get; set; } = [];

    public Airport() { }
    public Airport(string iCAO, string name, string countryCode, string city, GPS coordinate)
    {
      ICAO = iCAO;
      Name = name;
      CountryCode = countryCode;
      City = city;
      Coordinate = coordinate;
      Runways = [];
    }
  }
}
