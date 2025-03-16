using Eng.EFsExtensions.Modules.FlightLogModule.SimBriefModel;
using ESystem.Asserting;
using ESystem.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.FlightLogModule.VatsimModel
{
  class VatsimProvider
  {
    public static List<FlightPlan> LoadFromJson(string filePath)
    {
      try
      {
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<FlightPlan>>(json) ?? new List<FlightPlan>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error loading JSON file: {ex.Message}");
        return new List<FlightPlan>();
      }
    }

    public static async Task<List<FlightPlan>> LoadFromUrlAsync(string vatsimId)
    {
      EAssert.Argument.IsNonEmptyString(vatsimId, nameof(vatsimId));

      string url = $"https://api.vatsim.net/v2/members/{vatsimId}/flightplans";
      using HttpClient client = new();
      string json = await client.GetStringAsync(url);
      return JsonConvert.DeserializeObject<List<FlightPlan>>(json) ?? new List<FlightPlan>();
    }

    internal static RunViewModel.RunModelVatsimCache? CreateData(string vatsimId)
    {
      var plans = LoadFromUrlAsync(vatsimId).GetAwaiter().GetResult();
      var plan = plans.First();
      RunViewModel.RunModelVatsimCache ret = new RunViewModel.RunModelVatsimCache(
        plan.FlightType, plan.Callsign, plan.Aircraft.Split("/")[0], plan.Dep, plan.Arr, plan.Alt, plan.Route, plan.Altitude,
        ConvertHHMMToDateTime(plan.DeptTime), new TimeSpan(plan.HrsEnroute, plan.MinEnroute, 0));

      return ret;
    }

    private static DateTime ConvertHHMMToDateTime(string deptTime)
    {
      EAssert.IsTrue(deptTime.Length == 4, $"{nameof(deptTime)} must be in HHMM format.");
      var hh = deptTime.Substring(0, 2);
      var mm = deptTime.Substring(2);

      return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(hh), int.Parse(mm), 0);
    }
  }
}
