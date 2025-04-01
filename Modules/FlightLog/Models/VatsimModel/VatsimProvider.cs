using Eng.EFsExtensions.Modules.FlightLogModule.Models;
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

      string url = $"http://api.vatsim.net/v2/members/{vatsimId}/flightplans";
      using var client = new HttpClient();
      using var stream = await client.GetStreamAsync(url);
      using var streamReader = new StreamReader(stream);
      string json = await streamReader.ReadToEndAsync();
      return JsonConvert.DeserializeObject<List<FlightPlan>>(json) ?? new List<FlightPlan>();
    }

    internal static RunViewModel.RunModelVatsimCache? CreateData(string vatsimId)
    {
      var downloadTask = Task.Run(async () => await LoadFromUrlAsync(vatsimId));
      var plans = downloadTask.Result;
      var plan = plans.First();
      RunViewModel.RunModelVatsimCache ret = new(
        plan.FlightType == "IFR" ? FlightRules.IFR : plan.FlightType == "VFR" ? FlightRules.VFR : throw new ApplicationException("Unexpected VATSIM flight type " + plan.FlightType + ". Expected IFR/VFR."), 
        plan.Callsign, plan.Aircraft.Split("/")[0], plan.GetRegistration(), plan.Dep, plan.Arr, plan.Alt, plan.Route, 
        int.Parse(plan.Altitude), int.Parse(plan.CruiseSpeed),
        plan.GetDepartureDateTime(), plan.GetEnrouteTime(), plan.GetFuelDurationTime());

      return ret;
    }
  }
}
