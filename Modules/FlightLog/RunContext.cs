using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.SimBriefModel;
using Eng.EFsExtensions.Modules.FlightLogModule.VatsimModel;
using ESimConnect;
using ESystem.Asserting;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class RunContext : NotifyPropertyChanged
  {
    private const double FUEL_LITRES_TO_KG = 0.8;
    private class SimPropValues
    {
      private const int EMPTY_TYPE_ID = -1;
      private readonly ESimConnect.Extenders.ValueCacheExtender cache;

      private readonly TypeId[] engRunningTypeId = new TypeId[] { new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID) };
      private readonly TypeId[] wheelOnGroundTypeId = new TypeId[] { new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID) };

      private readonly TypeId parkingBrakeTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId heightTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId latitudeTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId longitudeTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId iasTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId fuelQuantityLtrsTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownBankDegreesTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownLatitudeTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownLongitudeTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownPitchDegreesTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownVelocityTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId emptyWeightKgTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId totalWeightKgTypeId = new(EMPTY_TYPE_ID);
      private readonly RequestId atcIdRequestId = new RequestId(EMPTY_TYPE_ID);

      public SimPropValues(NewSimObject simObject)
      {
        this.cache = simObject.ExtValue;
        this.parkingBrakeTypeId = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.BrakesAndLandingGear.BRAKE_PARKING_POSITION);
        this.heightTypeId = cache.Register(
          ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND,
          ESimConnect.Definitions.SimUnits.Length.FOOT);
        this.latitudeTypeId = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE);
        this.longitudeTypeId = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE);

        this.engRunningTypeId[0] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "1");
        this.engRunningTypeId[1] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "2");
        this.engRunningTypeId[2] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "3");
        this.engRunningTypeId[3] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "4");

        this.wheelOnGroundTypeId[0] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "0");
        this.wheelOnGroundTypeId[1] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "1");
        this.wheelOnGroundTypeId[2] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "2");

        this.iasTypeId = cache.Register(
          ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED,
          ESimConnect.Definitions.SimUnits.Speed.KNOT);
        this.fuelQuantityLtrsTypeId = cache.Register("FUEL TOTAL QUANTITY", ESimConnect.Definitions.SimUnits.Volume.LITER); // weights Kgs not working
        this.touchdownBankDegreesTypeId = cache.Register("PLANE TOUCHDOWN BANK DEGREES");
        this.touchdownLatitudeTypeId = cache.Register("PLANE TOUCHDOWN LATITUDE");
        this.touchdownLongitudeTypeId = cache.Register("PLANE TOUCHDOWN LONGITUDE");
        this.touchdownPitchDegreesTypeId = cache.Register("PLANE TOUCHDOWN PITCH DEGREES");
        this.touchdownVelocityTypeId = cache.Register("PLANE TOUCHDOWN NORMAL VELOCITY");

        this.emptyWeightKgTypeId = cache.Register("EMPTY WEIGHT", ESimConnect.Definitions.SimUnits.Weight.KILOGRAM);
        this.totalWeightKgTypeId = cache.Register("TOTAL WEIGHT", ESimConnect.Definitions.SimUnits.Weight.KILOGRAM);



        var simCon = simObject.ESimCon;
        simCon.DataReceived += ESimCon_DataReceived;
        TypeId typeId;
        typeId = simCon.Strings.Register("ATC ID", ESimConnect.ESimConnect.StringsHandler.StringLength._8);
        atcIdRequestId = simCon.Strings.Request(typeId);
      }

      private void ESimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
      {
        if (e.RequestId == atcIdRequestId)
          this.AtcId = (string)e.Data;
      }
      public string? AtcId { get; private set; }
      public bool ParkingBrakeSet => cache.GetValue(parkingBrakeTypeId) == 1;
      public double Height => cache.GetValue(heightTypeId);
      public double Latitude => cache.GetValue(latitudeTypeId);
      public double Longitude => cache.GetValue(longitudeTypeId);
      public bool IsAnyEngineRunning => engRunningTypeId.Any(q => cache.GetValue(q) == 1);
      public bool IsAnyWheelOnGround => wheelOnGroundTypeId.Any(q => cache.GetValue(q) != 0);
      public double IAS => cache.GetValue(iasTypeId);
      public bool IsFlying => !IsAnyWheelOnGround;

      public double TotalFuelLtrs => cache.GetValue(fuelQuantityLtrsTypeId);

      public double TouchdownBankDegrees => cache.GetValue(touchdownBankDegreesTypeId);
      public double TouchdownLatitude => cache.GetValue(touchdownLatitudeTypeId);
      public double TouchdownLongitude => cache.GetValue(touchdownLongitudeTypeId);
      public double TouchdownPitchDegrees => cache.GetValue(touchdownPitchDegreesTypeId);
      public double TouchdownVelocity => cache.GetValue(touchdownVelocityTypeId);

      public int EmptyWeightKg => (int)cache.GetValue(emptyWeightKgTypeId);
      public int TotalWeightKg => (int)cache.GetValue(totalWeightKgTypeId);
    }

    private SimPropValues simPropValues = null!;
    private readonly Settings settings = null!;
    private readonly List<Airport> airports;
    private readonly NewSimObject simObj;
    private readonly Logger logger;
    private readonly LogFlightsManager flightsManager;

    public RunContext(InitContext initContext, Settings settings)
    {
      EAssert.Argument.IsNotNull(initContext, nameof(initContext));
      EAssert.IsNotNull(initContext.SelectedProfile, "SelectedProfile not set.");

      this.logger = Logger.Create(this);
      this.simObj = NewSimObject.GetInstance();

      this.RunVM = new RunViewModel();

      this.RunVM.Profile = initContext.SelectedProfile;
      this.RunVM.Clear();
      this.settings = settings;
      this.airports = settings.Airports;

      this.flightsManager = LogFlightsManager.Init(settings.DataFolder);

      // DEBUG STUFF, DELETE LATER
      //UpdateSimbriefAndVatsimIfRequiredAsync();
      //this.RunVM.StartUpCache = new RunViewModel.RunModelStartUpCache(DateTime.Now, 49000, 174 * 95, 5500, 11, 22);
      //this.RunVM.TakeOffCache = new RunViewModel.RunModelTakeOffCache(DateTime.Now, 5200, 137, 11, 22);
      //this.RunVM.LandingCache = new RunViewModel.RunModelLandingCache(DateTime.Now, 2100, 120, 3.023, 11, 22, 121, 3.423, 11, 22);
      //this.RunVM.ShutDownCache = new RunViewModel.RunModelShutDownCache(DateTime.Now, 2000, 11, 22);

      this.simObj.ExtOpen.OpenInBackground(() => this.simPropValues = new SimPropValues(this.simObj));
    }

    public RunViewModel RunVM
    {
      get { return base.GetProperty<RunViewModel>(nameof(RunVM))!; }
      set { base.UpdateProperty(nameof(RunVM), value); }
    }

    private void CheckForNextState()
    {
      if (this.simPropValues == null) return; // not initialized yet
      switch (RunVM.State)
      {
        case RunViewModel.RunModelState.WaitingForStartup:
          ProcessWaitForOffBlocks();
          break;
        case RunViewModel.RunModelState.StartedWaitingForTakeOff:
          ProcessWaitForTakeOff();
          break;
        case RunViewModel.RunModelState.InFlightWaitingForLanding:
          ProcssWaitForLanding();
          break;
        case RunViewModel.RunModelState.LandedWaitingForShutdown:
          LandedWaitingForShutdown();
          break;
        default:
          throw new ESystem.Exceptions.UnexpectedEnumValueException(RunVM.State);
      }
    }

    private void LandedWaitingForShutdown()
    {
      if (!this.simPropValues.ParkingBrakeSet) return;
      if (this.simPropValues.IsAnyEngineRunning) return;

      this.RunVM.ShutDownCache = new(DateTime.Now, (int)(this.simPropValues.TotalFuelLtrs * FUEL_LITRES_TO_KG),
        this.simPropValues.Latitude, this.simPropValues.Longitude);
      LogFlight logFlight = GenerateLogFlight(this.RunVM);

      this.flightsManager.StoreFlight(logFlight);
    }

    private LogFlight GenerateLogFlight(RunViewModel runVM)
    {
      EAssert.IsNotNull(runVM.StartUpCache, "StartUpCache not set.");
      EAssert.IsNotNull(runVM.TakeOffCache, "TakeOffCache not set.");
      EAssert.IsNotNull(runVM.LandingCache, "LandingCache not set.");
      EAssert.IsNotNull(runVM.ShutDownCache, "ShutDownCache not set.");

      string? departureICAO = runVM.SimBriefCache?.DepartureICAO
        ?? RunVM.VatsimCache?.DepartureICAO
        ?? GetAirportByCoordinates(runVM.StartUpCache!.Latitude, runVM.StartUpCache!.Longitude);
      string? destinationICAO = runVM.SimBriefCache?.DestinationICAO
        ?? RunVM.VatsimCache?.DestinationICAO
        ?? GetAirportByCoordinates(runVM.LandingCache!.Latitude, runVM.LandingCache!.Longitude);
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
      double airDistance = runVM.SimBriefCache?.AirDistanceNM ?? TryGetAirDistance(departureICAO, destinationICAO);
      double? routeDistance = runVM.SimBriefCache?.RouteDistanceNM;

      LogStartUp startUp = new(
        runVM.SimBriefCache?.OffBlockPlannedTime ?? runVM.VatsimCache?.PlannedDepartureTime, runVM.StartUpCache.Time,
        (int)runVM.StartUpCache.FuelKg,
        new GPS(runVM.StartUpCache.Latitude, runVM.StartUpCache.Longitude));

      LogTakeOff takeOff = new(
        runVM.SimBriefCache?.TakeOffPlannedTime, runVM.TakeOffCache.Time,
        runVM.SimBriefCache?.EstimatedTakeOffFuelKg, runVM.TakeOffCache.FuelKg,
        new GPS(runVM.TakeOffCache.Latitude, runVM.TakeOffCache.Longitude),
        (int)runVM.TakeOffCache.IAS);

      LogLanding landing = new(
        runVM.SimBriefCache?.LandingPlannedTime, runVM.LandingCache!.Time,
        runVM.SimBriefCache?.EstimatedLandingFuelKg, runVM.LandingCache!.FuelKg,
        new GPS(runVM.LandingCache!.TouchdownLatitude, runVM.LandingCache!.TouchdownLongitude),
        (int)runVM.LandingCache!.IAS,
        runVM.LandingCache!.TouchdownVelocity, runVM.LandingCache!.TouchdownPitchDegrees);

      LogShutDown shutDown = new(
        runVM.SimBriefCache?.OnBlockPlannedTime, runVM.ShutDownCache.Time, runVM.ShutDownCache!.FuelKg,
        new GPS(runVM.ShutDownCache!.Latitude, runVM.ShutDownCache!.Longitude));

      DivertReason divertReason = DivertReason.NotDiverted; //TODO this

      LogFlight ret = new(
        departureICAO, destinationICAO, alternateICAO, zfw, passengerCount, cargoWeight, fuelWeight,
        aircraftType, aircraftRegistration, aircraftModel,
        cruizeAltitude, airDistance, routeDistance,
        startUp, takeOff, landing, shutDown, divertReason);
      return ret;
    }

    private double TryGetAirDistance(string? departureIcao, string? destinationIcao)
    {
      double ret;
      if (departureIcao == null || destinationIcao == null)
        ret = double.NaN;
      else
      {
        Airport? depAirport = airports.FirstOrDefault(q => q.ICAO == departureIcao);
        Airport? destAirport = airports.FirstOrDefault(q => q.ICAO == destinationIcao);
        if (depAirport == null || destAirport == null)
          ret = Double.NaN;
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

      if (minDist != null && minDist.Distance < 5) // some close airport
        ret = minDist.Airport.ICAO;
      else
        ret = null;

      return ret;
    }

    private void ProcssWaitForLanding()
    {
      if (this.simPropValues.IsFlying) return;

      this.RunVM.LandingCache = new(DateTime.Now, (int)(this.simPropValues.TotalFuelLtrs * FUEL_LITRES_TO_KG),
        this.simPropValues.IAS,
        this.simPropValues.TouchdownBankDegrees, this.simPropValues.TouchdownLatitude, this.simPropValues.TouchdownLongitude,
        this.simPropValues.TouchdownVelocity, this.simPropValues.TouchdownPitchDegrees,
        this.simPropValues.Latitude, this.simPropValues.Longitude);
      this.RunVM.State = RunViewModel.RunModelState.LandedWaitingForShutdown;
    }

    private void ProcessWaitForTakeOff()
    {
      if (!this.simPropValues.IsFlying) return;

      this.RunVM.TakeOffCache = new(DateTime.Now, (int)(this.simPropValues.TotalFuelLtrs * FUEL_LITRES_TO_KG),
        this.simPropValues.IAS, this.simPropValues.Latitude, this.simPropValues.Longitude);
      UpdateSimbriefAndVatsimIfRequired();

      this.RunVM.State = RunViewModel.RunModelState.InFlightWaitingForLanding;
    }

    private void ProcessWaitForOffBlocks()
    {
      if (this.simPropValues.ParkingBrakeSet) return;

      if (RunVM.State == RunViewModel.RunModelState.AfterShutdown)
        this.RunVM.Clear();

      int emptyWeight = (int)(this.simPropValues.EmptyWeightKg);
      int totalWeight = (int)(this.simPropValues.TotalWeightKg);
      int fuelWeight = (int)(this.simPropValues.TotalFuelLtrs * FUEL_LITRES_TO_KG);
      int payloadAndCargoWeight = totalWeight - fuelWeight - emptyWeight;

      RunVM.StartUpCache = new(DateTime.Now, emptyWeight, payloadAndCargoWeight, fuelWeight,
        this.simPropValues.Latitude, this.simPropValues.Longitude);

      UpdateSimbriefAndVatsimIfRequired();

      RunVM.State = RunViewModel.RunModelState.StartedWaitingForTakeOff;
    }

    private Task UpdateSimbriefAndVatsimIfRequiredAsync()
    {
      Task t = new(() => UpdateSimbriefAndVatsimIfRequired());
      t.Start();
      return t;
    }

    private void UpdateSimbriefAndVatsimIfRequired()
    {
      //TODO do both in async:
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
