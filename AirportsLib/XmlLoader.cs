using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.Chlaot.Libs.AirportsLib
{
  public class XmlLoader
  {
    public static List<Airport> Load(string fileName)
    {
      List<Airport> ret;

      XmlSerializer serializer = new(typeof(List<Airport>));
      using StreamReader writer = new(fileName);
      var tmp = serializer.Deserialize(writer) ?? throw new UnexpectedNullException();
      ret = (List<Airport>)tmp;

      return ret;
    }
  }
}
