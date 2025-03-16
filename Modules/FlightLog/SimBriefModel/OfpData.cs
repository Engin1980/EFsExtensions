using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.FlightLogModule.SimBriefModel
{
  [XmlRoot("OFP")]
  public class OfpData
  {
    public Weights Weights { get; set; } = null!;
    public Aircraft Aircraft { get; set; } = null!;
    public Times Times { get; set; } = null!;
    public Fetch Fetch { get; set; } = null!;
    public Params Params { get; set; } = null!;
    public General General { get; set; } = null!;
    public Location Origin { get; set; } = null!;
    public Location Destination { get; set; } = null!;
    public Location Alternate { get; set; } = null!;
  }
}
