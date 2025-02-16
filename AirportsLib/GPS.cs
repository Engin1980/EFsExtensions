using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.Chlaot.Libs.AirportsLib
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
}
