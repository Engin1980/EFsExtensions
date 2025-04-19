using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using ESystem.Asserting;
using ESystem.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling
{
  public class ProfileManager
  {
    private readonly static Logger logger = Logger.Create("EFSE.Modules.FlightLog.ProfileManager");

    public static void SaveFlight(LoggedFlight logFlight, Profile profile)
    {
      string tmpFileName = System.IO.Path.GetTempFileName();

      string fileName = System.IO.Path.Combine(
        profile.Path,
        $"{logFlight.StartUpDateTime:yyyy-MM-dd-HH-mm-ss}_{logFlight.DepartureICAO}_{logFlight.DestinationICAO}.xml");
      XmlSerializer ser = new(typeof(LoggedFlight));
      try
      {
        using(FileStream fs = new System.IO.FileStream(tmpFileName, FileMode.Create))
        {
          ser.Serialize(fs, logFlight);
        }
        System.IO.File.Copy(tmpFileName, fileName);
        System.IO.File.Delete(tmpFileName);
      }
      catch (Exception ex)
      {
        // TODO handle better using logger
        throw new ApplicationException("Failed to save flight to log.", ex);
      }
    }

    public static List<Profile> GetAvailableProfiles(string dataPath)
    {
      EAssert.Argument.IsNonEmptyString(dataPath, nameof(dataPath));
      EAssert.Argument.IsTrue(System.IO.Directory.Exists(dataPath), nameof(dataPath), $"Directory '{dataPath}' does not exist.");

      var ret = System.IO.Directory.GetDirectories(dataPath)
        .Select(q => new Profile(System.IO.Path.GetFileName(q), q, System.IO.Directory.GetFiles(q, "*.xml").Length))
        .ToList();
      return ret;
    }

    public static List<LoggedFlight> GetProfileFlights(Profile profile)
    {
      EAssert.Argument.IsNotNull(profile, nameof(profile));
      EAssert.Argument.IsTrue(System.IO.Directory.Exists(profile.Path), nameof(profile), $"Directory '{profile.Path}' does not exist.");

      List<LoggedFlight> ret = new();

      var files = System.IO.Directory.GetFiles(profile.Path, "*.xml");
      foreach (var file in files)
      {
        XmlSerializer serializer = new(typeof(LoggedFlight));
        try
        {
          using System.IO.FileStream fileStream = new(file, System.IO.FileMode.Open);
          LoggedFlight? flight = (LoggedFlight?)(serializer.Deserialize(fileStream));
          EAssert.IsNotNull(flight);
          ret.Add(flight);
        }
        catch (Exception ex)
        {
          logger.Log(LogLevel.ERROR, $"Failed to deserialize '{file}'. Reason: " + ex.ToString());
        }
      }

      int i = 0;
      while (i < ret.Count)
      {
        var flight = ret[i];
        try
        {
          flight.CheckValidity(out bool resaveNeeded);
          if (resaveNeeded)
            SaveFlight(flight, profile);
        }
        catch (Exception ex)
        {
          logger.Log(LogLevel.ERROR, "Failed to check flight validity. Reason: " + ex.ToString());
          ret.RemoveAt(i);
          continue;
        }
        i++;
      }

      return ret;
    }

    public static StatsData GetFlightsStatsData(List<LoggedFlight> flights)
    {
      StatsData ret = LogStats.Calculate(flights);
      return ret;
    }

    public static Profile CreateProfile(string dataPath, string profileName)
    {
      EAssert.Argument.IsNonEmptyString(dataPath, nameof(dataPath));
      EAssert.Argument.IsTrue(System.IO.Directory.Exists(dataPath), nameof(dataPath), $"Directory '{dataPath}' does not exist.");
      EAssert.Argument.IsNonEmptyString(profileName, nameof(profileName));
      string fullProfilePath = System.IO.Path.Combine(dataPath, profileName);
      EAssert.Argument.IsTrue(System.IO.Directory.Exists(fullProfilePath) == false, nameof(profileName), $"Diretory '{fullProfilePath}' already exists.");

      System.IO.Directory.CreateDirectory(fullProfilePath);
      var ret = new Profile(profileName, fullProfilePath, 0);
      return ret;
    }
  }
}
