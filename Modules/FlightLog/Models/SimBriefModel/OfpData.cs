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
    [XmlElement("fuel")]
    public Fuel Fuel { get; set; } = null!;

    [XmlElement("weights")]
    public Weights Weights { get; set; } = null!;

    [XmlElement("aircraft")]
    public Aircraft Aircraft { get; set; } = null!;

    [XmlElement("times")]
    public Times Times { get; set; } = null!;

    [XmlElement("fetch")]
    public Fetch Fetch { get; set; } = null!;

    [XmlElement("params")]
    public Params Params { get; set; } = null!;

    [XmlElement("general")]
    public General General { get; set; } = null!;

    [XmlElement("origin")]
    public Location Origin { get; set; } = null!;

    [XmlElement("destination")]
    public Location Destination { get; set; } = null!;

    [XmlElement("alternate")]
    public Location Alternate { get; set; } = null!;

    [XmlElement("atc")]
    public Atc Atc { get; set; } = null!;
  }

}
