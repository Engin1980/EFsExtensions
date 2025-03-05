using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Libs.AirportsLib
{
  public class Runway
  {
    [XmlAttribute]
    public string Designator { get; set; } = "";
    public List<RunwayThreshold> Thresholds { get; set; } = new List<RunwayThreshold>();
    public int LengthInM { get; set; } = int.MinValue;
  }
}
