using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Libs.AirportsLib
{
  public class Airport
  {
    [XmlAttribute]
    public string ICAO { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountryCode { get; set; } = "";
    public string City { get; set; } = "";
    public GPS Coordinate { get; set; }
    public double Declination { get; set; }
    public List<Runway> Runways { get; set; } = new();
  }
}
