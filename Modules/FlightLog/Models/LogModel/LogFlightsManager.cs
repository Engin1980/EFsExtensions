using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public class LogFlightsManager
  {
    private readonly List<LogFlight> flights = new();
    private readonly string dataFolder;

    public event Action<LogFlight>? NewFlightLogged;
    public event Action? StatsUpdated;

    public IReadOnlyList<LogFlight> Flights => this.flights.AsReadOnly();
    public StatsData StatsData { get; private set; }

    public static LogFlightsManager Init(string dataFolder)
    {
      EAssert.Argument.IsNonEmptyString(dataFolder, nameof(dataFolder));
      EAssert.Argument.IsTrue(System.IO.Directory.Exists(dataFolder), nameof(dataFolder), $"Directory '{dataFolder}' does not exist.");

      LogFlightsManager ret = new(dataFolder);
      ret.ReloadFlighs();
      return ret;
    }

    internal void ReloadFlighs()
    {
      this.flights.Clear();

      var files = System.IO.Directory.GetFiles(dataFolder, "*.xml");
      XmlSerializer serializer = new(typeof(LogFlight));

      //TODO errors should be logged, not crashing apps
      foreach (var file in files)
      {
        using System.IO.FileStream fileStream = new(file, System.IO.FileMode.Open);
        LogFlight flight = (LogFlight)(serializer.Deserialize(fileStream) ?? throw new ApplicationException($"Failed to deserialize '{file}'"));
        
        flight.CheckValidity(out bool resaveNeeded);
        if (resaveNeeded)
          this.SaveFlight(flight);

        this.flights.Add(flight);
      }

      RecalculateStats();
    }

    internal void StoreNewFlight(LogFlight logFlight)
    {
      //TODO do some duplicity check here
      this.SaveFlight(logFlight);
      this.flights.Add(logFlight);
      this.NewFlightLogged?.Invoke(logFlight);
      RecalculateStats();
    }

    private void SaveFlight(LogFlight logFlight)
    {
      string fileName = System.IO.Path.Combine(
        this.dataFolder,
        $"{logFlight.StartUpDateTime:yyyy-mm-dd-hh-mm-ss}_{logFlight.DepartureICAO}_{logFlight.DestinationICAO}.xml");
      XmlSerializer ser = new(typeof(LogFlight));
      try
      {
        ser.Serialize(System.IO.File.Create(fileName), logFlight);
      }
      catch (Exception ex)
      {
        // TODO handle
        throw new ApplicationException();
      }
    }

    public LogFlightsManager(string dataFolder)
    {
      this.dataFolder = dataFolder;
      this.StatsData = null!;
      RecalculateStats();
    }

    private void RecalculateStats()
    {
      this.StatsData = LogStats.Calculate(this.flights);
      this.StatsUpdated?.Invoke();
    }
  }
}
