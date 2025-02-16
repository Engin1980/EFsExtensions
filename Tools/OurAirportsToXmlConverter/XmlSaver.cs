using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Serialization;

namespace OurAirportsToXmlConverter
{
  internal class XmlSaver
  {
    public static void Save(List<Airport> airports, string fileName)
    {
      XmlSerializer serializer = new(typeof(List<Airport>));
      using StreamWriter writer = new(fileName);
      serializer.Serialize(writer, airports);
    }
  }
}
