using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Libs.AirportsLib
{
  public class XmlLoader
  {
    public static List<Airport> Load(string fileName, bool skipEmptyAirports = true)
    {
      List<Airport> ret;

      XmlSerializer serializer = new(typeof(List<Airport>));
      using StreamReader writer = new(fileName);
      var tmp = serializer.Deserialize(writer) ?? throw new UnexpectedNullException();
      ret = (List<Airport>)tmp;

      if (ret.Any(q => q.Runways.Count == 0 && skipEmptyAirports))
        ret = ret.Where(q => q.Runways.Count > 0).ToList();

      var invalids = ret.Where(q => q.Runways.Any(w => w.Thresholds.Count != 2)).ToList();
      if (invalids.Count > 0)
        throw new InvalidDataException($"Invalid runways (non-2-thresholds) in airports: {string.Join(", ", invalids.Select(q => q.ICAO))}");

      //TODO calculate heading for every threshold with heading==null as bearing between thresholds

      return ret;
    }
  }
}
