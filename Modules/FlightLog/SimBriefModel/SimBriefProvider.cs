using ESystem.Asserting;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.FlightLogModule.SimBriefModel
{
  public static class SimBriefProvider
  {
    public static OfpData LoadFromXml(string filePath)
    {
      XmlSerializer serializer = new(typeof(OfpData));
      using FileStream fileStream = new(filePath, FileMode.Open);
      return (OfpData)(serializer.Deserialize(fileStream) ?? throw new UnexpectedNullException());
    }

    public static async Task<OfpData> LoadFromUrlAsync(string simBriefId)
    {
      EAssert.Argument.IsNonEmptyString(simBriefId, nameof(simBriefId));

      string url = $"https://www.simbrief.com/api/xml.fetcher.php?userid={simBriefId}";
      using HttpClient client = new();
      string xmlContent = await client.GetStringAsync(url);
      using StringReader stringReader = new(xmlContent);
      XmlSerializer serializer = new(typeof(OfpData));
      return (OfpData)(serializer.Deserialize(stringReader) ?? throw new UnexpectedNullException());
    }

    internal static RunViewModel.RunModelSimDataCache CreateData(string simBriefId)
    {
      OfpData data = LoadFromUrlAsync(simBriefId).GetAwaiter().GetResult();
      RunViewModel.RunModelSimDataCache ret = new(
        data.Origin.IcaoCode, data.Destination.IcaoCode, data.Alternate.IcaoCode,
        ConvertEpochToDateTime(data.Times.SchedOut), ConvertEpochToDateTime(data.Times.SchedOff), ConvertEpochToDateTime(data.Times.SchedOn), ConvertEpochToDateTime(data.Times.SchedIn),
        data.General.InitialAltitude,
        data.General.AirDistance, data.General.RouteDistance,
        data.Aircraft.IcaoCode, data.Aircraft.Reg,
        data.Weights.PaxCount, data.Weights.Payload, data.Weights.Cargo, data.Weights.estZfw, data.Fuel.Taxi + data.Fuel.PlanTakeoff, data.Weights.estTow, data.Weights.estLw);
      return ret;
    }

    private static DateTime ConvertEpochToDateTime(long unixTimestamp)
    {
      return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
    }
  }
}
