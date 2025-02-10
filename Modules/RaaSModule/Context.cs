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
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Playing;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Shell;

namespace Eng.Chlaot.Modules.RaaSModule
{
  internal class Context : NotifyPropertyChanged
  {
    #region Private Classes + Structs + Interfaces

    private static class GpsCalculator
    {
      #region Private Fields

      private const double EarthRadiusKm = 6371.0;

      #endregion Private Fields

      // Earth's radius in kilometers

      #region Public Methods

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
        ret = Math.Abs(ret);
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

      #endregion Public Methods

      #region Private Methods

      private static double ToDegrees(double radians) => radians * (180 / Math.PI);

      private static double ToRadians(double degrees) => degrees * (Math.PI / 180);

      #endregion Private Methods
    }

    #endregion Private Classes + Structs + Interfaces

    #region Private Fields

    private const int HOLDING_POINT_SHIFT_DISTANCE = 100;
    private const int MIN_DISTANCE_TO_DO_EVALUATIONS_IN_M = 8_000;
    private const int MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_ON_GROUND_IN_M = 50;
    private const int MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_IN_FLIGHT_IN_M = 350;
    private const int MAX_HEADING_DELTA_TO_BE_ALIGNED_ON_RUNWAY = 15;
    private readonly object __simDataUnsafeLock = new();
    private readonly Logger logger;
    private readonly ESimConnect.ESimConnect simConnect;
    private readonly System.Timers.Timer timer;
    private readonly Action<bool> updateReadyFlag;
    private bool isBusy = false;
    private RunwayThreshold? lastLandingThreshold = null;
    private RunwayThreshold? lastLineUpThreshold = null;
    private Runway? lastTaxiToRunwayRunway = null;
    private Synthetizer? synthetizer;
    private RunwayThreshold? lastDistanceThreshold = null;
    private List<RaasDistance>? lastDistanceThresholdRemainingDistances = null;

    #endregion Private Fields

    #region Public Properties

    public List<Airport> Airports
    {
      get { return base.GetProperty<List<Airport>>(nameof(Airports))!; }
      set { base.UpdateProperty(nameof(Airports), value); }
    }

    public MetaInfo MetaInfo
    {
      get { return base.GetProperty<MetaInfo>(nameof(MetaInfo))!; }
      set { base.UpdateProperty(nameof(MetaInfo), value); }
    }

    public Raas RaaS
    {
      get { return base.GetProperty<Raas>(nameof(RaaS))!; }
      set { base.UpdateProperty(nameof(RaaS), value); }
    }

    public Settings Settings { get; set; } = new Settings();

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

    #endregion Public Properties

    #region Public Constructors

    public Context(Logger logger, Action<bool> updateReadyFlag)
    {
      this.logger = logger;
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

    #endregion Public Constructors

    #region Public Methods

    public void Start()
    {
      this.synthetizer = Synthetizer.CreateDefault(); //TODO load from settings

      this.timer.Enabled = true;
    }

    public void Stop()
    {

    }

    #endregion Public Methods

    #region Internal Methods

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

    #endregion Internal Methods

    #region Private Methods

    private void EvaluateAirborneRaas()
    {
      Debug.Assert(this.RuntimeData.NearestAirport != null);
      var airport = this.RuntimeData.NearestAirport.Airport;

      var tmpR = this.RuntimeData.NearestRunways; //TODO calculate the next only for the closest runway?
      var tmpT = tmpR.SelectMany(q => q.Runway.Thresholds, (r, t) => new
      {
        Runway = r.Runway,
        Threshold = t,
        ShiftDistance = r.ShiftDistance,
        ThresholdDistance = GpsCalculator.GetDistance(
          t.Coordinate.Latitude, t.Coordinate.Longitude,
          SimData.latitude, SimData.longitude),
        Bearing = GpsCalculator.InitialBearing(
          SimData.latitude, SimData.longitude,
          t.Coordinate.Latitude, t.Coordinate.Longitude)
      });
      this.RuntimeData.Landing = tmpT
        .Select(q => new LandingRaasData(airport, q.Runway, q.Threshold, q.ShiftDistance, q.ThresholdDistance, (Heading)q.Bearing))
        .OrderBy(q => q.ShiftDistance).ThenBy(q => q.ThresholdDistance)
        .ToList();

      var thrsCandidate = this.RuntimeData.Landing.First() ?? throw new UnexpectedNullException();

      if (thrsCandidate.ThresholdDistance > RaaS.Speeches.LandingRunway.Distance.GetInMeters())
      {
        RuntimeData.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} threshold-distance {thrsCandidate.ThresholdDistance} over limit {RaaS.Speeches.LandingRunway.Distance.GetInMeters()}";
      }
      else if (thrsCandidate.ShiftDistance > MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_IN_FLIGHT_IN_M)
      {
        RuntimeData.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} shift-distance {thrsCandidate.ShiftDistance} over limit {MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_IN_FLIGHT_IN_M}";
      }
      else
      {
        if (lastLandingThreshold != thrsCandidate.Threshold)
        {
          lastLandingThreshold = thrsCandidate.Threshold;
          Say(RaaS.Speeches.LandingRunway, thrsCandidate.Threshold);
          RuntimeData.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} announced";
        }
        else
        {
          RuntimeData.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} already announced";
        }
      }
    }
    public record NearestAirport(Airport Airport, double Distance, List<RunwayShifts> RunwayShifts);
    public record NearestRunways(Runway Runway, double ShiftDistance);
    public record RunwayShifts(Runway Runway, double ShiftDistance);
    public record LandingRaasData(Airport Airport, Runway Runway, RunwayThreshold Threshold, double ShiftDistance, double ThresholdDistance, Heading Bearing);
    public record GroundRaasTaxiData(Airport Airport, Runway Runway, double ShiftDistance);
    public record GroundRaasLineUpData(Airport Airport, Runway Runway, RunwayThreshold Threshold, Heading Bearing, double Distance, double DeltaHeading);

    public class RuntimeDataBox : NotifyPropertyChanged
    {
      public NearestAirport? NearestAirport
      {
        get { return base.GetProperty<NearestAirport?>(nameof(NearestAirport))!; }
        set { base.UpdateProperty(nameof(NearestAirport), value); }
      }

      public List<NearestRunways> NearestRunways
      {
        get { return base.GetProperty<List<NearestRunways>>(nameof(NearestRunways))!; }
        set { base.UpdateProperty(nameof(NearestRunways), value); }
      }

      public List<GroundRaasTaxiData> GroundTaxi
      {
        get { return base.GetProperty<List<GroundRaasTaxiData>>(nameof(GroundTaxi))!; }
        set { base.UpdateProperty(nameof(GroundTaxi), value); }
      }
      public string GroundTaxiStatus
      {
        get { return base.GetProperty<string>(nameof(GroundTaxiStatus))!; }
        set { base.UpdateProperty(nameof(GroundTaxiStatus), value); }
      }


      public List<GroundRaasLineUpData> GroundLineUp
      {
        get { return base.GetProperty<List<GroundRaasLineUpData>>(nameof(GroundLineUp))!; }
        set { base.UpdateProperty(nameof(GroundLineUp), value); }
      }
      public string GroundLineUpStatus
      {
        get { return base.GetProperty<string>(nameof(GroundLineUpStatus))!; }
        set { base.UpdateProperty(nameof(GroundLineUpStatus), value); }
      }

      public List<LandingRaasData> Landing
      {
        get { return base.GetProperty<List<LandingRaasData>>(nameof(Landing))!; }
        set { base.UpdateProperty(nameof(Landing), value); }
      }
      public string LandingStatus
      {
        get { return base.GetProperty<string>(nameof(LandingStatus))!; }
        set { base.UpdateProperty(nameof(LandingStatus), value); }
      }


      public string DistanceStatus
      {
        get { return base.GetProperty<string>(nameof(DistanceStatus))!; }
        set { base.UpdateProperty(nameof(DistanceStatus), value); }
      }

    }
    public RuntimeDataBox RuntimeData { get; set; } = new();


    private void EvaluateGroundTaxiRaas()
    {
      Debug.Assert(RuntimeData.NearestAirport != null);

      RuntimeData.GroundTaxi = RuntimeData.NearestRunways
        .Select(q => new GroundRaasTaxiData(
          RuntimeData.NearestAirport.Airport,
          q.Runway,
          q.ShiftDistance))
        .OrderBy(q => q.ShiftDistance)
        .ToList();

      var grtd = RuntimeData.GroundTaxi.First();
      if (grtd.ShiftDistance < RaaS.Speeches.TaxiToRunway.Distance.GetInMeters())
      {
        if (lastTaxiToRunwayRunway != grtd.Runway)
        {
          lastTaxiToRunwayRunway = grtd.Runway;
          var closestThreshold = grtd.Runway.Thresholds
            .MinBy(q => GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude))
            ?? throw new UnexpectedNullException();
          Say(RaaS.Speeches.TaxiToRunway, closestThreshold);
        }
        else
        {
          RuntimeData.GroundTaxiStatus = $"Threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} already announced";
        }
      }
      else
      {
        RuntimeData.GroundTaxiStatus = $"Threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} shift-distance {grtd.ShiftDistance} over threshold {RaaS.Speeches.TaxiToRunway.Distance.GetInMeters()}";
      }
    }
    private void EvaluateGroundLineUpRaas()
    {
      Debug.Assert(RuntimeData.NearestAirport != null);

      var rwyWithMinDistance = this.RuntimeData.NearestRunways.First();
      if (rwyWithMinDistance.ShiftDistance > MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_ON_GROUND_IN_M)
      {
        RuntimeData.GroundLineUpStatus = $"Threshold {RuntimeData.NearestAirport.Airport.ICAO}/{rwyWithMinDistance.Runway.Designator} shift-distance {rwyWithMinDistance.ShiftDistance} over threshold {MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_ON_GROUND_IN_M}";
      }
      else
      {
        this.RuntimeData.GroundLineUp = rwyWithMinDistance.Runway.Thresholds
          .Select(q => new
          {
            Threshold = q,
            Bearing = GpsCalculator.InitialBearing(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude),
            Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude)
          })
          .Select(q => new GroundRaasLineUpData(
            RuntimeData.NearestAirport.Airport,
            rwyWithMinDistance.Runway,
            q.Threshold,
            (Heading)q.Bearing,
            q.Distance,
            Math.Abs((double)q.Threshold.Heading! - (double)SimData.Heading)))
          .OrderBy(q => q.DeltaHeading)
          .ToList();

        var thresholdCandidate = this.RuntimeData.GroundLineUp.MinBy(q => q.DeltaHeading) ?? throw new UnexpectedNullException();
        if (thresholdCandidate.DeltaHeading < HOLDING_POINT_SHIFT_DISTANCE)
        {
          if (lastLineUpThreshold != thresholdCandidate.Threshold)
          {
            lastLineUpThreshold = thresholdCandidate.Threshold;
            Say(RaaS.Speeches.OnRunway, thresholdCandidate.Threshold);
            RuntimeData.GroundLineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} announced";
          }
          else
          {
            RuntimeData.GroundLineUpStatus =
              $"Threshold {thresholdCandidate.Airport.ICAO}/{thresholdCandidate.Threshold.Designator} already announced";
          }
        }
      }
    }
    private void EvaluateDistanceRaas()
    {
      Debug.Assert(RuntimeData.NearestAirport != null);

      var airport = RuntimeData.NearestAirport.Airport;

      var candidateRwy = RuntimeData.NearestRunways.First();
      if (candidateRwy.ShiftDistance > MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_ON_GROUND_IN_M)
      {
        RuntimeData.DistanceStatus = $"{airport.ICAO}/{candidateRwy.Runway.Designator} shift-distance {candidateRwy.ShiftDistance} over threshold {MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_ON_GROUND_IN_M}m";
        return;
      }

      var tmps = candidateRwy.Runway.Thresholds
        .Select(q => new
        {
          Threshold = q,
          DeltaBearing = Math.Abs((double)(SimData.Heading - (double)((Heading)q.Heading! + 180))) //TODO rewrite to be valid w.r.t headings
        })
        .ToList();
      tmps = tmps
        .Where(q => q.DeltaBearing < MAX_HEADING_DELTA_TO_BE_ALIGNED_ON_RUNWAY)
        .OrderBy(q => q.DeltaBearing)
        .ToList();

      if (tmps.Count == 0)
      {
        RuntimeData.DistanceStatus =
          $"{airport.ICAO}/{candidateRwy.Runway.Designator} no threshold within {MAX_HEADING_DELTA_TO_BE_ALIGNED_ON_RUNWAY} degrees bearing-delta";
        return;
      }

      var candidate = tmps.First();
      var dist = GpsCalculator.GetDistance(candidate.Threshold.Coordinate.Latitude, candidate.Threshold.Coordinate.Longitude, SimData.latitude, SimData.longitude);

      if (candidate.Threshold != lastDistanceThreshold)
      {
        lastDistanceThreshold = candidate.Threshold;
        lastDistanceThresholdRemainingDistances = RaaS.Speeches.DistanceRemaining.Distances.OrderBy(q => q.GetInMeters()).ToList();
      }

      var candidateDistances = lastDistanceThresholdRemainingDistances!
        .Where(q => q.GetInMeters() > dist)
        .OrderBy(q => q.GetInMeters());
      if (!candidateDistances.Any()) return;
      var candidateDistance = candidateDistances.First();
      lastDistanceThresholdRemainingDistances!.RemoveAll(q => q.GetInMeters() >= candidateDistance.GetInMeters());

      Say(RaaS.Speeches.DistanceRemaining, candidateDistance);
    }



    private void EvaluateRaas()
    {
      if (SimData.height > 3000) return; // too high, do nothing

      EvaluateNearestAirport();
      if (RuntimeData.NearestAirport != null && RuntimeData.NearestAirport.Distance < MIN_DISTANCE_TO_DO_EVALUATIONS_IN_M)
      {
        EvaluateNearestRunways();
        if (SimData.height < 10)
        {
          EvaluateGroundTaxiRaas();
          EvaluateGroundLineUpRaas();
          EvaluateDistanceRaas();
        }
        else
        {
          EvaluateAirborneRaas();
        }
      }
    }

    private void EvaluateNearestRunways()
    {
      RuntimeData.NearestRunways = RuntimeData.NearestAirport!.Airport.Runways
        .Select(q => new NearestRunways(
          q,
          GpsCalculator.GetDistanceFromLine(
            q.Thresholds[0].Coordinate.Latitude,
            q.Thresholds[0].Coordinate.Longitude,
            q.Thresholds[1].Coordinate.Latitude,
            q.Thresholds[1].Coordinate.Longitude,
            SimData.latitude, SimData.longitude)))
        .OrderBy(q => q.ShiftDistance)
        .ToList();
    }

    private void EvaluateNearestAirport()
    {
      Airport? tmpA = null;
      double? tmpD = null;
      if (this.RuntimeData.NearestAirport != null)
      {
        tmpD = GpsCalculator.GetDistance(
          this.RuntimeData.NearestAirport.Airport.Coordinate.Latitude,
          this.RuntimeData.NearestAirport.Airport.Coordinate.Longitude,
          SimData.latitude, SimData.longitude);
        if (tmpD.Value < this.RuntimeData.NearestAirport.Distance)
          tmpA = this.RuntimeData.NearestAirport.Airport;
      }
      if (tmpA == null)
      {
        var closeAirports = Airports
        .Where(q => q.Coordinate.Latitude > SimData.latitude - 1)
        .Where(q => q.Coordinate.Latitude < SimData.latitude + 1)
        .Where(q => q.Coordinate.Longitude > SimData.longitude - 1)
        .Where(q => q.Coordinate.Longitude < SimData.longitude + 1);

        var tmpAD = closeAirports
          .Select(q => new
          {
            Airport = q,
            Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, SimData.latitude, SimData.longitude)
          })
          .MinBy(q => q.Distance)
          ?? throw new UnexpectedNullException();
        tmpA = tmpAD.Airport;
        tmpD = tmpAD.Distance;
      }

      Debug.Assert(tmpA != null && tmpD != null);

      if (tmpD > MIN_DISTANCE_TO_DO_EVALUATIONS_IN_M)
      {
        RuntimeData.NearestAirport = new NearestAirport(tmpA, tmpD.Value, new List<RunwayShifts>());
      }
      else
      {
        var rwys = tmpA.Runways
          .Select(q => new RunwayShifts(q, GpsCalculator.GetDistanceFromLine(
              q.Thresholds[0].Coordinate.Latitude,
              q.Thresholds[0].Coordinate.Longitude,
              q.Thresholds[1].Coordinate.Latitude,
              q.Thresholds[1].Coordinate.Longitude,
              SimData.latitude, SimData.longitude)))
          .OrderBy(q => q.ShiftDistance)
          .ToList();
        RuntimeData.NearestAirport = new NearestAirport(tmpA, tmpD.Value, rwys);
      }
    }

    private void CheckReadyStatus()
    {
      this.updateReadyFlag(this.RaaS != null && this.Airports.Count > 0);
    }

    private void Say(RaasSpeech speech, RunwayThreshold threshold)
    {
      string d = string.Join(" ", threshold.Designator.ToArray());
      d = d.Replace("L", "Left").Replace("R", "Right").Replace("C", "Center");
      string s = speech.Speech.Replace("%rwy", d);

      logger.Log(LogLevel.INFO, "Saying: " + s);

      Debug.Assert(synthetizer != null);
      var bytes = synthetizer!.Generate(s);
      Player player = new(bytes);
      player.Play();
    }

    private void Say(RaasDistancesSpeech speech, RaasDistance candidateDistance)
    {
      string s = speech.Speech;
      s = s.Replace("%dist", candidateDistance.Value + " " + candidateDistance.Unit switch
      {
        RaasDistance.RaasDistanceUnit.km => "kilometers",
        RaasDistance.RaasDistanceUnit.m => "meters",
        RaasDistance.RaasDistanceUnit.ft => "feet",
        RaasDistance.RaasDistanceUnit.nm => "miles",
        _ => throw new UnexpectedEnumValueException(candidateDistance.Unit)
      });

      var bytes = synthetizer!.Generate(s);
      Player player = new(bytes);
      player.Play();
    }

    private void simConnect_Connected(ESimConnect.ESimConnect eSimCon)
    {
      this.logger.Log(LogLevel.INFO, "Connected to SimConnect");
    }

    private void simConnect_DataReceived(
      ESimConnect.ESimConnect eSimCon,
      ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      var data = (SimDataStruct)e.Data;
      this.SimData = data;
      this.logger.Log(LogLevel.DEBUG, "Received data from SimConnect");
    }

    private void simConnect_Disconnected(ESimConnect.ESimConnect eSimCon)
    {
      this.logger.Log(LogLevel.INFO, "Disconnected from SimConnect");
    }

    private void simConnect_ThrowsException(ESimConnect.ESimConnect eSimCon, SimConnectException ex)
    {
      this.logger.Log(LogLevel.ERROR, "SimConnect exception: " + ex.ToString());
    }

    private void timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      if (isBusy) return;
      isBusy = true;
      if (!simConnect.IsOpened)
      {
        TryConnect();
      }
      else
      {
        try
        {
          EvaluateRaas();
        }
        catch (Exception ex)
        {
          logger.Log(LogLevel.ERROR, "Error in EvaluateRaas: " + ex.ToString());
        }
      }
      isBusy = false;
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

    #endregion Private Methods
  }
}
