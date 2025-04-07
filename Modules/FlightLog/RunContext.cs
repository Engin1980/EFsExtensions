using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using ESimConnect;
using ESystem.Asserting;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Eng.EFsExtensions.Modules.FlightLogModule.Models;
using System.Runtime.CompilerServices;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Profiling;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.ActiveFlight.VatsimModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.ActiveFlight.SimBriefModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.Shared;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public partial class RunContext : NotifyPropertyChanged
  {
    private const double FUEL_LITRES_TO_KG = 0.8;

    private LandingDetector? landingDetector = null;
    private TakeOffDetector? takeoffDetector = null;
    private SimPropValues simPropValues = null!;
    private readonly Settings settings = null!;
    private readonly List<Airport> airports;
    private readonly NewSimObject simObj;
    private readonly Logger logger;
    private readonly Profile selectedProfile;

    public RunContext(InitContext initContext, Settings settings)
    {
      EAssert.Argument.IsNotNull(initContext, nameof(initContext));
      EAssert.IsNotNull(initContext.SelectedProfile, "SelectedProfile not set.");

      this.logger = Logger.Create(this);
      this.simObj = NewSimObject.GetInstance();

      this.RunVM = new ActiveFlightViewModel
      {
        Profile = initContext.SelectedProfile
      };
      this.RunVM.Clear();
      this.settings = settings;
      this.airports = settings.Airports;
      this.selectedProfile = initContext.SelectedProfile;

      this.LogVM = new();



      this.simObj.ExtOpen.OpenInBackground(() => this.simPropValues = new SimPropValues(this.simObj));
    }

    public ActiveFlightViewModel RunVM
    {
      get { return base.GetProperty<ActiveFlightViewModel>(nameof(RunVM))!; }
      set { base.UpdateProperty(nameof(RunVM), value); }
    }


    public object LogVM
    {
      get => base.GetProperty<object>(nameof(LogVM))!;
      set => base.UpdateProperty(nameof(LogVM), value);
    }

    private void CheckForNextState()
    {
      if (this.simPropValues == null) return; // not initialized yet
      switch (RunVM.State)
      {
        case ActiveFlightViewModel.RunModelState.WaitingForStartupAfterShutdown:
        case ActiveFlightViewModel.RunModelState.WaitingForStartupForTheFirstTime:
          ProcessWaitForOffBlocks();
          break;
        case ActiveFlightViewModel.RunModelState.StartedWaitingForTakeOff:
          ProcessWaitForTakeOff();
          break;
        case ActiveFlightViewModel.RunModelState.InFlightWaitingForLanding:
          ProcessWaitForLanding();
          break;
        case ActiveFlightViewModel.RunModelState.LandedWaitingForShutdown:
          ProcessLandedWaitingForShutdown();
          break;
        default:
          throw new ESystem.Exceptions.UnexpectedEnumValueException(RunVM.State);
      }
    }

    private LoggedFlight GenerateLogFlight(ActiveFlightViewModel runVM)
    {
      EAssert.IsNotNull(runVM.StartUpCache, "StartUpCache not set.");
      EAssert.IsNotNull(runVM.TakeOffCache, "TakeOffCache not set.");
      EAssert.IsNotNull(runVM.LandingCache, "LandingCache not set.");
      EAssert.IsNotNull(runVM.ShutDownCache, "ShutDownCache not set.");
      EAssert.IsNotNull(runVM.TakeOffAttempt, "TakeOffAttempt not set.");
      EAssert.IsNotNull(runVM.LandingAttempts, "LandingAttempts not set.");
      EAssert.IsTrue(runVM.LandingAttempts.Count > 0, "LandingAttempts is empty.");

      InvalidateSimBriefOrVatsimIfRequired(runVM);

      List<LoggedFlightTouchdown> touchdowns = CollectTouchdowns();

      string callsign = runVM.SimBriefCache?.Callsign ?? runVM.VatsimCache?.Callsign ?? this.simPropValues?.AtcId ?? "???";

      string? departureICAO = runVM.SimBriefCache?.DepartureICAO
        ?? RunVM.VatsimCache?.DepartureICAO
        ?? GetAirportByCoordinates(runVM.StartUpCache!.Latitude, runVM.StartUpCache!.Longitude);
      string? destinationICAO = runVM.SimBriefCache?.DestinationICAO
        ?? RunVM.VatsimCache?.DestinationICAO
        ?? GetAirportByCoordinates(runVM.ShutDownCache.Latitude, runVM.ShutDownCache.Longitude);
      string? alternateICAO = runVM.SimBriefCache?.AlternateICAO
        ?? RunVM.VatsimCache?.AlternateICAO
        ?? null;
      double zfw = runVM.SimBriefCache?.ZfwKg ?? RunVM.StartUpCache!.ZFW;
      int? passengerCount = RunVM.SimBriefCache?.NumberOfPassengers;
      int? cargoWeight = RunVM.SimBriefCache?.Cargo;

      int? fuelWeight = RunVM.SimBriefCache?.FuelKg ?? RunVM.StartUpCache!.FuelKg;
      string aircraftType = RunVM.SimBriefCache?.AirplaneType ?? RunVM.VatsimCache?.Aircraft ?? "UNSET"; //TODO get from FS
      string aircraftRegistration = runVM.SimBriefCache?.AirplaneRegistration ?? RunVM.VatsimCache?.Registration ?? "UNSET"; //TODO get from FS
      string? aircraftModel = RunVM.SimBriefCache?.AirplaneType ?? RunVM.VatsimCache?.Aircraft;
      int cruizeAltitude = runVM.SimBriefCache?.Altitude ?? runVM.VatsimCache?.PlannedFlightLevel ?? runVM.MaxAchievedAltitude;
      double distance = runVM.SimBriefCache?.AirDistanceNM * 1.852  // is in NM
        ?? TryGetAirDistance(departureICAO, destinationICAO) / 1_000 // is in m
        ?? GpsCalculator.GetDistance(runVM.StartUpCache!.Latitude, runVM.StartUpCache!.Longitude, runVM.ShutDownCache!.Latitude, runVM.ShutDownCache!.Longitude) / 1_000;

      DivertReason divertReason = DivertReason.NotDiverted; //TODO this

      FlightRules flightRules = runVM.SimBriefCache?.FlightRules ?? runVM.VatsimCache?.FlightType ?? FlightRules.Unknown;

      LoggedFlightTakeOff takeOff = new()
      {
        RunStartDateTime = runVM.TakeOffAttempt.RollStartDateTime,
        AirborneDateTime = runVM.TakeOffAttempt.AirborneDateTime,
        RunStartLocation = new GPS(runVM.TakeOffAttempt.RollStartLatitude, runVM.TakeOffAttempt.RollStartLongitude),
        AirborneLocation = new GPS(runVM.TakeOffAttempt.TakeOffLatitude, runVM.TakeOffAttempt.TakeOffLongitude),
        MaxVS = runVM.TakeOffAttempt.MaxVS,
        IAS = (int)runVM.TakeOffAttempt.IAS,
        GS = (int)runVM.TakeOffAttempt.GS,
        MaxBank = runVM.TakeOffAttempt.MaxBank,
        MaxPitch = runVM.TakeOffAttempt.MaxPitch,
        MaxAccY = runVM.TakeOffAttempt.MaxAccY,
        FrontGearTime = runVM.TakeOffAttempt.RollToFrontGearTime,
        AllGearTime = runVM.TakeOffAttempt.RollToAllGearTime
      };

      LoggedFlight ret = new()
      {
        AircraftModel = aircraftModel,
        AircraftRegistration = aircraftRegistration,
        AircraftType = aircraftType,
        Distance = distance,
        CargoWeight = cargoWeight,
        Callsign = callsign,
        CruizeAltitude = cruizeAltitude,
        DestinationICAO = destinationICAO,
        DepartureICAO = departureICAO,
        AlternateICAO = alternateICAO,
        DivertReason = divertReason,
        FlightRules = flightRules,
        LandingFuelWeight = runVM.LandingCache!.FuelKg,
        LandingScheduledFuelWeight = runVM.SimBriefCache?.EstimatedLandingFuelKg,
        LandingScheduledDateTime = runVM.SimBriefCache?.LandingPlannedTime,
        PassengerCount = passengerCount,
        ShutDownScheduledDateTime = runVM.SimBriefCache?.OnBlockPlannedTime,
        ShutDownFuelWeight = runVM.ShutDownCache!.FuelKg,
        ShutDownLocation = new GPS(runVM.ShutDownCache!.Latitude, runVM.ShutDownCache!.Longitude),
        ShutDownDateTime = runVM.ShutDownCache.Time,
        StartupLocation = new GPS(runVM.StartUpCache.Latitude, runVM.StartUpCache.Longitude),
        StartUpDateTime = runVM.StartUpCache.Time,
        StartUpScheduledDateTime = runVM.SimBriefCache?.OffBlockPlannedTime ?? runVM.VatsimCache?.PlannedDepartureTime,
        StartUpFuelWeight = runVM.StartUpCache.FuelKg,
        TakeOffFuelWeight = runVM.TakeOffCache.FuelKg,
        TakeOff = takeOff,
        TakeOffScheduledFuelWeight = runVM.SimBriefCache?.EstimatedTakeOffFuelKg,
        TakeOffScheduledDateTime = runVM.SimBriefCache?.TakeOffPlannedTime,
        Touchdowns = touchdowns,
        ZFW = zfw
      };
      return ret;
    }

    private List<LoggedFlightTouchdown> CollectTouchdowns()
    {
      List<LoggedFlightTouchdown> ret = new();

      List<List<ActiveFlightViewModel.LandingAttemptData>> groups = new();
      DateTime last = DateTime.MinValue;
      for (int i = 0; i < this.RunVM.LandingAttempts.Count; i++)
      {
        var la = this.RunVM.LandingAttempts[i];
        if (i == 0)
        {
          var tmp = new List<ActiveFlightViewModel.LandingAttemptData>()
          {
            la
          };
          groups.Add(tmp);
        }
        else if (la.TouchDownDateTime.Subtract(last).TotalSeconds < 20)
        {
          groups.Last().Add(la);
        }
        else
        {
          var tmp = new List<ActiveFlightViewModel.LandingAttemptData>
          {
            la
          };
          groups.Add(tmp);
        }
        last = la.TouchDownDateTime;
      }

      foreach (var group in groups)
      {
        ActiveFlightViewModel.LandingAttemptData f = group.First();
        ActiveFlightViewModel.LandingAttemptData l = group.Last();
        LoggedFlightTouchdown lt = new()
        {
          TouchDownDateTime = f.TouchDownDateTime,
          TouchDownLocation = new GPS(f.TouchDownLatitude, f.TouchDownLongitude),
          RollOutEndDateTime = l.RollOutEndDateTime,
          RollOutEndLocation = l.RollOutEndDateTime == null ? null : new GPS(l.RollOutEndLatitude!.Value, l.RollOutEndLongitude!.Value),
          IAS = (int)Math.Round(f.IAS),
          VS = f.VS,
          GS = (int)Math.Round(f.GS),
          Bank = f.Bank,
          Pitch = f.Pitch,
          MaxAccY = group.Max(q => q.MaxAccY),
          AllGearTime = l.AllGearTime,
          MainGearTime = l.MainGearTime
        };
        ret.Add(lt);
      }
      return ret;
    }

    private void InvalidateSimBriefOrVatsimIfRequired(ActiveFlightViewModel runVM)
    {
      EAssert.IsNotNull(runVM.StartUpCache, "StartUpCache not set.");

      if (runVM.SimBriefCache != null)
      {
        TimeSpan timeDiff = runVM.SimBriefCache.OffBlockPlannedTime - runVM.StartUpCache.Time;
        if (Math.Abs(timeDiff.TotalHours) > 1)
        {
          logger.Log(LogLevel.WARNING, $"SimBrief offblock time is {timeDiff.TotalHours} hours off actual offblock time. SimBrief ignored.");
          runVM.SimBriefCache = null;
        }
      }
      if (runVM.SimBriefCache != null)
      {
        double? simBriefDistance = TryGetDistance(runVM.SimBriefCache.DepartureICAO, runVM.StartUpCache.Latitude, runVM.StartUpCache.Longitude);
        if (simBriefDistance == null || simBriefDistance > 7000)
        {
          logger.Log(LogLevel.WARNING, $"SimBrief departure airport distance is {simBriefDistance} km from actual departure airport. SimBrief ignored.");
          runVM.SimBriefCache = null;
        }
      }
      if (runVM.VatsimCache != null)
      {
        TimeSpan timeDiff = runVM.VatsimCache.PlannedDepartureTime - runVM.StartUpCache.Time;
        if (Math.Abs(timeDiff.TotalHours) > 1)
        {
          logger.Log(LogLevel.WARNING, $"VATSIM offblock time is {timeDiff.TotalHours} hours off actual offblock time. VATSIM ignored.");
          runVM.VatsimCache = null;
        }
      }
      if (runVM.VatsimCache != null)
      {
        double? vatsimDistance = TryGetDistance(runVM.VatsimCache.DepartureICAO, runVM.StartUpCache.Latitude, runVM.StartUpCache.Longitude);
        if (vatsimDistance == null || vatsimDistance > 7000)
        {
          logger.Log(LogLevel.WARNING, $"VATSIM departure airport distance is {vatsimDistance} km from actual departure airport. VATSIM ignored.");
          runVM.VatsimCache = null;
        }
      }
    }

    private double? TryGetDistance(string icao, double latitude, double longitude)
    {
      Airport? airport = airports.FirstOrDefault(q => q.ICAO == icao);
      if (airport == null) return null;

      return GpsCalculator.GetDistance(airport.Coordinate.Latitude, airport.Coordinate.Longitude, latitude, longitude);
    }

    private double? TryGetAirDistance(string? departureIcao, string? destinationIcao)
    {
      double? ret;
      if (departureIcao == null || destinationIcao == null)
        ret = null;
      else
      {
        Airport? depAirport = airports.FirstOrDefault(q => q.ICAO == departureIcao);
        Airport? destAirport = airports.FirstOrDefault(q => q.ICAO == destinationIcao);
        if (depAirport == null || destAirport == null)
          ret = null;
        else
          ret = GpsCalculator.GetDistance(depAirport.Coordinate, destAirport.Coordinate);
      }
      return ret;
    }

    private string? GetAirportByCoordinates(double latitude, double longitude)
    {
      string? ret;

      double minLat, maxLat, minLon, maxLon;
      (minLat, maxLat) = (latitude - 1, latitude + 1);
      (minLon, maxLon) = (longitude - 1, longitude + 1);
      var tmp = airports
        .Where(q => q.Coordinate.Latitude > minLat && q.Coordinate.Latitude < maxLat)
        .Where(q => q.Coordinate.Longitude > minLon && q.Coordinate.Longitude < maxLon)
        .ToList();
      var dsts = tmp
        .Select(q => new { Airport = q, Distance = GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, latitude, longitude) })
        .OrderBy(q => q.Distance)
        .ToList();
      var minDist = dsts.FirstOrDefault();

      if (minDist != null && minDist.Distance < (5 * 1_000)) // some close airport
        ret = minDist.Airport.ICAO;
      else
        ret = null;

      return ret;
    }

    private void ProcessWaitForLanding()
    {
      if (this.simPropValues.IsFlying)
      {
        if (this.landingDetector == null && this.simPropValues.Height < 125) //TODO remove magic numbers
        {
          this.landingDetector = new(this.simObj, this.RunVM);
          this.landingDetector.AttemptRecorded += r => this.RunVM.LandingAttempts.Add(r);
          this.landingDetector.InitAndStart();
        }
        else if (this.landingDetector != null && this.simPropValues.Height > 175)
        {
          this.landingDetector.Stop();
          this.landingDetector = null;
        }
        return;
      }

      this.RunVM.LandingCache = new(DateTime.UtcNow, (int)(this.simPropValues.TotalFuelLtrs * FUEL_LITRES_TO_KG),
        this.simPropValues.IAS, this.simPropValues.Latitude, this.simPropValues.Longitude);
      this.RunVM.State = ActiveFlightViewModel.RunModelState.LandedWaitingForShutdown;
    }

    private void ProcessWaitForTakeOff()
    {
      if (!this.simPropValues.IsFlying) return;

      this.RunVM.TakeOffCache = new(DateTime.UtcNow, (int)(this.simPropValues.TotalFuelLtrs * FUEL_LITRES_TO_KG),
        this.simPropValues.IAS, this.simPropValues.Latitude, this.simPropValues.Longitude);
      UpdateSimbriefAndVatsimIfRequiredAsync();

      this.RunVM.State = ActiveFlightViewModel.RunModelState.InFlightWaitingForLanding;
    }

    private void ProcessWaitForOffBlocks()
    {
      if (this.simPropValues.SmartParkingBrakeSet) return;

      if (RunVM.State == ActiveFlightViewModel.RunModelState.WaitingForStartupAfterShutdown)
        this.RunVM.Clear();

      int emptyWeight = (int)(this.simPropValues.EmptyWeightKg);
      int totalWeight = (int)(this.simPropValues.TotalWeightKg);
      int fuelWeight = (int)(this.simPropValues.TotalFuelLtrs * FUEL_LITRES_TO_KG);
      int payloadAndCargoWeight = totalWeight - fuelWeight - emptyWeight;

      RunVM.StartUpCache = new(DateTime.UtcNow, emptyWeight, payloadAndCargoWeight, fuelWeight,
        this.simPropValues.Latitude, this.simPropValues.Longitude);

      UpdateSimbriefAndVatsimIfRequiredAsync();

      this.takeoffDetector = new(this.simObj, this.RunVM);
      this.takeoffDetector.AttemptRecorded += r =>
      {
        this.RunVM.TakeOffAttempt = r;
        this.takeoffDetector.Stop();
        this.takeoffDetector = null;
      };
      this.takeoffDetector.InitAndStart();
      RunVM.State = ActiveFlightViewModel.RunModelState.StartedWaitingForTakeOff;
    }

    private void ProcessLandedWaitingForShutdown()
    {
      if (!this.simPropValues.SmartParkingBrakeSet) return;
      if (this.simPropValues.IsAnyEngineRunning) return;
      if (this.simPropValues.IsFlying)
      {
        // got airborne after landing
        this.RunVM.State = ActiveFlightViewModel.RunModelState.InFlightWaitingForLanding;
        return;
      }

      if (this.landingDetector != null)
      {
        this.landingDetector.Stop();
        this.landingDetector = null;
      }

      this.RunVM.ShutDownCache = new(DateTime.UtcNow, (int)(this.simPropValues.TotalFuelLtrs * FUEL_LITRES_TO_KG),
        this.simPropValues.Latitude, this.simPropValues.Longitude);
      LoggedFlight logFlight = GenerateLogFlight(this.RunVM);

      ProfileManager.SaveFlight(logFlight, selectedProfile);
      this.RunVM.Clear();

      this.RunVM.State = ActiveFlightViewModel.RunModelState.WaitingForStartupForTheFirstTime;
    }

    private Task UpdateSimbriefAndVatsimIfRequiredAsync()
    {
      Task t = new(() => UpdateSimbriefAndVatsimIfRequired());
      t.Start();
      return t;
    }

    private void UpdateSimbriefAndVatsimIfRequired()
    {
      if (this.RunVM.VatsimCache == null && this.settings.VatsimId != null)
        try
        {
          RunVM.VatsimCache = VatsimProvider.CreateData(this.settings.VatsimId);
          logger.Log(LogLevel.INFO, "VATSIM flight plan downloaded.");
        }
        catch (Exception ex)
        {
          logger.Log(LogLevel.ERROR, "VATSIM flight plan download failed. " + ex.Message);
        }
      if (this.RunVM.SimBriefCache == null && this.settings.SimBriefId != null)
        try
        {
          RunVM.SimBriefCache = SimBriefProvider.CreateData(this.settings.SimBriefId);
          logger.Log(LogLevel.INFO, "SimBrief flight plan downloaded.");
        }
        catch (Exception ex)
        {
          logger.Log(LogLevel.ERROR, "SimBrief flight plan download failed. " + ex.Message);
        }
    }

    internal void Start()
    {
      this.simObj.ExtTime.SimSecondElapsed += () => CheckForNextState();
    }
  }
}
