using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.Libs.AirportsLib;
using Eng.Chlaot.Modules.RaaSModule.Model;
using ESystem;
using ESystem.Miscelaneous;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ESystem.Functions.TryCatch;
using System.Xml.Linq;
using ChlaotModuleBase.ModuleUtils.SimConWrapping;
using ESimConnect;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using System.Timers;
using System.Reflection.Metadata.Ecma335;
using System.Speech.Synthesis;
using ESystem.Exceptions;

namespace Eng.Chlaot.Modules.RaaSModule
{
  internal class Context : NotifyPropertyChanged
  {
    private const int DO_NOT_CHECK_CLOSER_AIRPORT_IF_BELOW_THIS_DISTANCE_IN_KM = 8; //KM
    private const int IS_ON_RUNWAY_CENTER_DISTANCE_IN_M = 15;
    private readonly Logger logger;
    private readonly Action<bool> updateReadyFlag;
    private readonly object __simDataUnsafeLock = new();
    private readonly System.Timers.Timer timer;
    private readonly ESimConnect.ESimConnect simConnect;
    public SimDataStruct SimData
    {
      get
      {
        SimDataStruct ret;
        lock (this.__simDataUnsafeLock)
        {
          ret = GetProperty<SimDataStruct>(nameof(SimData))!;
        }
        return ret;
      }
      set
      {
        lock (this.__simDataUnsafeLock)
        {
          UpdateProperty<SimDataStruct>(nameof(SimData), value);
        }
      }
    }

    public MetaInfo MetaInfo
    {
      get { return base.GetProperty<MetaInfo>(nameof(MetaInfo))!; }
      set { base.UpdateProperty(nameof(MetaInfo), value); }
    }

    public List<Airport> Airports
    {
      get { return base.GetProperty<List<Airport>>(nameof(Airports))!; }
      set { base.UpdateProperty(nameof(Airports), value); }
    }

    public Raas RaaS
    {
      get { return base.GetProperty<Raas>(nameof(RaaS))!; }
      set { base.UpdateProperty(nameof(RaaS), value); }
    }

    public Context(Action<bool> updateReadyFlag)
    {
      this.logger = Logger.Create(this);
      this.updateReadyFlag = updateReadyFlag;
      this.timer = new(1000)
      {
        AutoReset = true,
        Enabled = false,
      };
      this.timer.Elapsed += timer_Elapsed;

      this.simConnect = new();
      simConnect.Connected += simConnect_Connected;
      simConnect.ThrowsException += simConnect_ThrowsException;
      simConnect.Disconnected += simConnect_Disconnected;
      simConnect.DataReceived += simConnect_DataReceived;
    }

    internal void LoadAirportsFile(string recentXmlFile)
    {
      this.Airports = XmlLoader.Load(recentXmlFile);
      this.CheckReadyStatus();
    }

    internal void LoadRaasFile(string xmlFile)
    {
      try
      {
        logger.Invoke(LogLevel.INFO, $"Checking file '{xmlFile}'");
        try
        {
          XmlUtils.ValidateXmlAgainstXsd(xmlFile, new string[] {
            @".\xmls\xsds\Global.xsd",
            @".\xmls\xsds\RaasSchema.xsd"}, out List<string> errors);
          if (errors.Any())
            throw new ApplicationException("XML does not match XSD: " + string.Join("; ", errors.Take(5)));
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Failed to validate XML file against XSD. Error: " + ex.Message, ex);
        }

        logger.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        XDocument doc = Try(() => XDocument.Load(xmlFile, LoadOptions.SetLineInfo),
          ex => throw new ApplicationException($"Unable to load xml file '{xmlFile}'.", ex));

        MetaInfo tmpMeta = MetaInfo.Deserialize(doc);
        Raas raas = Try(() => RaasXmlLoader.Load(doc),
          ex => throw new ApplicationException("Unable to read/deserialize copilot-set from '{xmlFile}'. Invalid file content?", ex));

        logger.Invoke(LogLevel.INFO, $"Checking sanity");
        Try(
          () => raas.CheckSanity(),
          ex => throw new ApplicationException("Error loading failures.", ex));

        this.MetaInfo = tmpMeta;
        this.RaaS = raas;

        this.CheckReadyStatus();
        //this.LastLoadedFile = xmlFile;
        logger.Invoke(LogLevel.INFO, $"RaaS set file '{xmlFile}' successfully loaded.");
      }
      catch (Exception ex)
      {
        this.updateReadyFlag(false);
        logger.Invoke(LogLevel.ERROR, $"Failed to load failure set from '{xmlFile}'." + ex.GetFullMessage());
      }
    }

    private void CheckReadyStatus()
    {
      this.updateReadyFlag(this.RaaS != null && this.Airports.Count > 0);
    }

    public void Start()
    {
      this.timer.Enabled = true;
    }

    private void TryConnect()
    {
      try
      {
        logger.Log(LogLevel.DEBUG, "Opening simConnect");
        simConnect.Open();
      }
      catch (Exception ex)
      {
        logger.Log(LogLevel.ERROR, "Failed to open simConnect: " + ex.ToString());
        return;
      }

      logger.Log(LogLevel.DEBUG, "Registering simConnect type");
      simConnect.Structs.Register<SimDataStruct>();

      logger.Log(LogLevel.DEBUG, "Registering simConnect repeated-requests");
      simConnect.Structs.RequestRepeatedly<SimDataStruct>(SimConnectPeriod.SECOND, sendOnlyOnChange: true);
    }

    private void timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      if (!simConnect.IsOpened)
      {
        TryConnect();
      }
      else
      {
        EvaluateRaas();
      }
    }

    private void EvaluateRaas()
    {
      if (SimData.height > 3000) return; // too high, do nothing
      if (SimData.height < 10)
        EvaluateGroundRaas();
      else
        EvaluateAirborneRaas();
    }

    private void EvaluateAirborneRaas()
    {
      var airport = GetNearestAirport();
      var rwysData = airport.Runways
        .Select(q => new
        {
          Runway = q,
          PerpendicularDistance = GpsCalculator.GetDistanceFromLine(
            q.Thresholds[0].Coordinate.Latitude,
            q.Thresholds[0].Coordinate.Longitude,
            q.Thresholds[1].Coordinate.Latitude,
            q.Thresholds[1].Coordinate.Longitude,
            SimData.latitude, SimData.longitude)
        });
      var rwyCandidate = rwysData.MinBy(q => q.PerpendicularDistance) ?? throw new UnexpectedNullException();
      var thrsData = rwyCandidate.Runway.Thresholds.Select(q => new
      {
        Threshold = q,
        Distance = GpsCalculator.GetDistance(
          q.Coordinate.Latitude, q.Coordinate.Longitude,
          SimData.latitude, SimData.longitude)
      });

      var thrsCandidate = thrsData.MinBy(q => q.Distance) ?? throw new UnexpectedNullException();

      if (thrsCandidate.Distance < RaaS.Speeches.LandingRunway.Distance.GetInMeters())
      {
        if (lastLandingThreshold != thrsCandidate.Threshold)
        {
          lastLandingThreshold = thrsCandidate.Threshold;
          Say(RaaS.Speeches.LandingRunway, thrsCandidate.Threshold);
        }
      }
    }

    private void Say(RaasSpeech speech, RunwayThreshold threshold)
    {
      string d = string.Join(" ", threshold.Designator.ToArray());
      d = d.Replace("L", "Left").Replace("R", "Right").Replace("C", "Center");
      string s = speech.Speech.Replace("%rwy", d);

      logger.Log(LogLevel.INFO, "Saying: " + s);
    }

    private RunwayThreshold? lastLandingThreshold = null;
    private Airport? lastNearestAirport;
    private double lastNearestAirportDistance;
    private Airport GetNearestAirport()
    {
      if (lastNearestAirport != null)
      {
        var newDist = GpsCalculator.GetDistance(lastNearestAirport.Coordinate.Latitude, lastNearestAirport.Coordinate.Longitude, SimData.latitude, SimData.longitude);
        if (newDist < DO_NOT_CHECK_CLOSER_AIRPORT_IF_BELOW_THIS_DISTANCE_IN_KM * 1000 || newDist < lastNearestAirportDistance)
        {
          lastNearestAirportDistance = newDist;
          return lastNearestAirport;
        }
      }

      var closeAirports = Airports
        .Where(q => q.Coordinate.Latitude > SimData.latitude - 1 && q.Coordinate.Latitude < SimData.latitude + 1)
        .Where(q => q.Coordinate.Longitude > SimData.longitude - 1 && q.Coordinate.Longitude < SimData.longitude + 1);

      var closestAirportWithDistance = closeAirports
        .Select(q => new { Airport = q, Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude) })
        .MinBy(q => q.Distance)
        ?? throw new ApplicationException("No airport found in the vicinity of the aircraft.");
      lastNearestAirport = closestAirportWithDistance.Airport;
      lastNearestAirportDistance = closestAirportWithDistance.Distance;
      return lastNearestAirport;
    }

    private static class GpsCalculator
    {
      private const double EarthRadiusKm = 6371.0; // Earth's radius in kilometers

      public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
      {
        const double R = 6371; // Radius of the Earth in kilometers
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distanceInM = R * c * 1000; // Distance in ometers

        return distanceInM;
      }

      public static double GetDistanceFromLine(double lat1, double lon1, double lat2, double lon2, double latP, double lonP, char unit = 'K')
      {
        double d13 = GetDistance(lat1, lon1, latP, lonP) / EarthRadiusKm; // Distance from P to A (normalized)
        double brng13 = InitialBearing(lat1, lon1, latP, lonP);
        double brng12 = InitialBearing(lat1, lon1, lat2, lon2);

        double ret = Math.Asin(Math.Sin(d13) * Math.Sin(ToRadians(brng13 - brng12))) * EarthRadiusKm;
        return ret;
      }

      public static double InitialBearing(double lat1, double lon1, double lat2, double lon2)
      {
        double dLon = ToRadians(lon2 - lon1);
        double phi1 = ToRadians(lat1);
        double phi2 = ToRadians(lat2);

        double y = Math.Sin(dLon) * Math.Cos(phi2);
        double x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(dLon);

        return (ToDegrees(Math.Atan2(y, x)) + 360) % 360;
      }

      private static double ToRadians(double degrees) => degrees * (Math.PI / 180);
      private static double ToDegrees(double radians) => radians * (180 / Math.PI);
    }

    private Runway? lastTaxiToRunwayRunway = null;
    private RunwayThreshold? lastLineUpThreshold = null;
    private void EvaluateGroundRaas()
    {
      var airport = GetNearestAirport();
      var rwys = airport.Runways;

      var rwyWithMinDistance = rwys
        .Select(q => new
        {
          Runway = q,
          Distance = GpsCalculator.GetDistanceFromLine(
          q.Thresholds[0].Coordinate.Latitude,
          q.Thresholds[0].Coordinate.Longitude,
          q.Thresholds[1].Coordinate.Latitude,
          q.Thresholds[1].Coordinate.Longitude,
          SimData.latitude, SimData.longitude)
        })
        .MinBy(q => q.Distance)
        ?? throw new ApplicationException("Unexpectingly, no runway found in the vicinity of the aircraft.");

      if (rwyWithMinDistance.Distance < RaaS.Speeches.TaxiToRunway.Distance.GetInMeters())
      {
        if (lastTaxiToRunwayRunway != rwyWithMinDistance.Runway)
        {
          lastTaxiToRunwayRunway = rwyWithMinDistance.Runway;
          var closerThreshold = rwyWithMinDistance.Runway.Thresholds
            .MinBy(q => GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude));
          Say(RaaS.Speeches.TaxiToRunway, closerThreshold);
        }
      }
      else if (rwyWithMinDistance.Distance < IS_ON_RUNWAY_CENTER_DISTANCE_IN_M)
      {
        //TODO remove distance if not used
        var thresholdsWithBearingsAndDistances = rwyWithMinDistance.Runway.Thresholds
          .Select(q => new
          {
            Threshold = q,
            Bearing = GpsCalculator.InitialBearing(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude),
            Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude)
          });

        var thresholdsWithBearingAndDistancesAndHeadingDelta = thresholdsWithBearingsAndDistances
          .Select(q => new { q.Threshold, q.Bearing, q.Distance, HeadingDelta = Math.Abs(q.Bearing - SimData.Heading) });

        var thresholdCandidate = thresholdsWithBearingAndDistancesAndHeadingDelta.MinBy(q => q.HeadingDelta);

        if (thresholdCandidate.HeadingDelta < 10)
        {
          if (lastLineUpThreshold != thresholdCandidate.Threshold)
          {
            lastLineUpThreshold = thresholdCandidate.Threshold;
            Say(RaaS.Speeches.OnRunway, thresholdCandidate.Threshold);
          }
        }
      }
    }
    private void simConnect_DataReceived(
      ESimConnect.ESimConnect eSimCon,
      ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      var data = (SimDataStruct)e.Data;
      this.SimData = data;
      this.logger.Log(LogLevel.DEBUG, "Received data from SimConnect");
    }

    private void simConnect_ThrowsException(ESimConnect.ESimConnect eSimCon, SimConnectException ex)
    {
      this.logger.Log(LogLevel.ERROR, "SimConnect exception: " + ex.ToString());
    }

    private void simConnect_Disconnected(ESimConnect.ESimConnect eSimCon)
    {
      this.logger.Log(LogLevel.INFO, "Disconnected from SimConnect");
    }

    private void simConnect_Connected(ESimConnect.ESimConnect eSimCon)
    {
      this.logger.Log(LogLevel.INFO, "Connected to SimConnect");
    }

    public void Stop()
    {

    }
  }
}
