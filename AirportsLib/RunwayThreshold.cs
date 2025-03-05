using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Libs.AirportsLib
{
  public class RunwayThreshold
  {
    [XmlAttribute]
    public string Designator { get; set; } = "";
    public GPS Coordinate { get; set; } = new GPS();
    public double? Elevation { get; set; } = null;
    public double? Heading { get; set; } = null;
    public double? DisplacedThresholdFt { get; set; } = null;
  }
}
