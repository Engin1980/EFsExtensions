using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.Chlaot.Libs.AirportsLib
{
  public class Runway
  {
    [XmlAttribute]
    public string Designator { get; set; } = "";
    public List<RunwayThreshold> Thresholds { get; set; } = new List<RunwayThreshold>();
  }
}
