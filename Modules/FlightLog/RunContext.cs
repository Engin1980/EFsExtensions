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
      private readonly TypeId fuelQuantityKgTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownBankDegrees = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownLatitude = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownLongitude = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownPitchDegrees = new(EMPTY_TYPE_ID);
      private readonly TypeId touchdownVelocity = new(EMPTY_TYPE_ID);

      public SimPropValues(ESimConnect.Extenders.ValueCacheExtender cache)
      {
        this.cache = cache;
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
        this.fuelQuantityKgTypeId = cache.Register("FUEL TOTAL QUANTITY", ESimConnect.Definitions.SimUnits.Weight.KILOGRAM);
        this.touchdownBankDegrees = cache.Register("PLANE TOUCHDOWN BANK DEGREES");
        this.touchdownLatitude = cache.Register("PLANE TOUCHDOWN LATITUDE");
        this.touchdownLongitude = cache.Register("PLANE TOUCHDOWN LONGITUDE");
        this.touchdownPitchDegrees = cache.Register("PLANE TOUCHDOWN PITCH DEGREES");
        this.touchdownVelocity = cache.Register("PLANE TOUCHDOWN NORMAL VELOCITY");
      }

      public bool ParkingBrakeSet => cache.GetValue(parkingBrakeTypeId) == 1;
      public double Height => cache.GetValue(heightTypeId);
      public double Latitude => cache.GetValue(latitudeTypeId);
      public double Longitude => cache.GetValue(longitudeTypeId);
      public bool IsAnyEngineRunning => engRunningTypeId.Any(q => cache.GetValue(q) == 1);
      public bool IsAnyWheelOnGround => wheelOnGroundTypeId.Any(q => cache.GetValue(q) != 0);
      public double IAS => cache.GetValue(iasTypeId);
      //public bool IsFlying => Height > 20 && IAS > 40;
      public bool IsFlying => !IsAnyWheelOnGround;

      public double TotalFuelKg => cache.GetValue(fuelQuantityKgTypeId);

      public double TouchdownBankDegrees => cache.GetValue(touchdownBankDegrees);
      public double TouchdownLatitude => cache.GetValue(touchdownLatitude);
      public double TouchdownLongitude => cache.GetValue(touchdownLongitude);
      public double TouchdownPitchDegrees => cache.GetValue(touchdownPitchDegrees);
      public double TouchdownVelocity => cache.GetValue(touchdownVelocity);
    }

    private SimPropValues simPropValues = null!;
    private readonly Settings settings = null!;
    private readonly List<Airport> airports;
    private readonly NewSimObject simObj;

    public RunContext(InitContext initContext, Settings settings)
    {
      EAssert.Argument.IsNotNull(initContext, nameof(initContext));
      EAssert.IsNotNull(initContext.SelectedProfile, "SelectedProfile not set.");

      this.simObj = NewSimObject.GetInstance();

      this.RunVM = new RunViewModel();

      this.RunVM.Profile = initContext.SelectedProfile;
      this.RunVM.Clear();
      this.settings = settings;
      this.airports = settings.Airports;
      this.simObj.ExtOpen.OpenInBackground(() => this.simPropValues = new SimPropValues(this.simObj.ExtValue));
    }

    internal RunViewModel RunVM
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

      this.RunVM.ShutDownCache = new(DateTime.Now, this.simPropValues.TotalFuelKg, this.simPropValues.Latitude, this.simPropValues.Longitude);
      LogFlight logFlight = GenerateLogFlight(this.RunVM);
    }

    private LogFlight GenerateLogFlight(RunViewModel runVM)
    {
      EAssert.IsNotNull(runVM.StartUpCache, "StartUpCache not set.");
      EAssert.IsNotNull(runVM.TakeOffCache, "TakeOffCache not set.");
      EAssert.IsNotNull(runVM.LandingCache, "LandingCache not set.");
      EAssert.IsNotNull(runVM.ShutDownCache, "ShutDownCache not set.");

      string? departureICAO = runVM.SimDataCache?.DepartureICAO
        ?? RunVM.VatsimCache?.DepartureICAO
        ?? GetAirportByCoordinates(runVM.StartUpCache!.Latitude, runVM.StartUpCache!.Longitude);
      string? destinationICAO = runVM.SimDataCache?.DestinationICAO
        ?? RunVM.VatsimCache?.DestinationICAO
        ?? GetAirportByCoordinates(runVM.LandingCache!.Latitude, runVM.LandingCache!.Longitude);
      string? alternateICAO = runVM.SimDataCache?.AlternateICAO
        ?? RunVM.VatsimCache?.AlternateICAO
        ?? null;
      double zfw = runVM.SimDataCache?.ZFW ?? double.NaN;
      int? passengerCount = RunVM.SimDataCache?.NumberOfPassengers;
      int? cargoWeight = RunVM.SimDataCache?.Cargo;
      int? fuelWeight = RunVM.SimDataCache?.TotalFuel;
      string aircraftType = RunVM.SimDataCache?.AirplaneType ?? RunVM.VatsimCache?.Aircraft ?? "UNSET"; //TODO get from FS
      string aircraftRegistration = runVM.SimDataCache?.AirplaneRegistration ?? RunVM.VatsimCache?.Registration ?? "UNSET"; //TODO get from FS
      string? aircraftModel = RunVM.SimDataCache?.AirplaneType ?? RunVM.VatsimCache?.Aircraft;
      int cruizeAltitude = runVM.SimDataCache?.Altitude ?? runVM.VatsimCache?.PlannedFlightLevel ?? runVM.MaxAchievedAltitude;
      double airDistance = runVM.SimDataCache?.AirDistanceNM ?? TryGetAirDistance(departureICAO, destinationICAO);
      double? routeDistance = runVM.SimDataCache?.RouteDistanceNM;

      LogStartUp startUp = new(
        runVM.SimDataCache?.OffBlockPlannedTime ?? runVM.VatsimCache?.PlannedDepartureTime, runVM.StartUpCache.Time,
        (int)runVM.StartUpCache.TotalFuel,
        new GPS(runVM.StartUpCache.Latitude, runVM.StartUpCache.Longitude));

      LogTakeOff takeOff = new(
        runVM.SimDataCache?.TakeOffPlannedTime, runVM.TakeOffCache.Time,
        runVM.SimDataCache?.EstimatedTOW, (int)runVM.TakeOffCache.TotalFuel,
        new GPS(runVM.TakeOffCache.Latitude, runVM.TakeOffCache.Longitude),
        (int)runVM.TakeOffCache.IAS);

      LogLanding landing = new(
        runVM.SimDataCache?.LandingPlannedTime, runVM.LandingCache!.Time,
        runVM.SimDataCache?.EstimatedLW, (int)runVM.LandingCache!.TotalFuel,
        new GPS(runVM.LandingCache!.TouchdownLatitude, runVM.LandingCache!.TouchdownLongitude),
        (int)runVM.LandingCache!.IAS,
        runVM.LandingCache!.TouchdownVelocity, runVM.LandingCache!.TouchdownPitchDegrees);

      LogShutDown shutDown = new(
        runVM.SimDataCache?.OnBlockPlannedTime, runVM.ShutDownCache.Time, (int)runVM.ShutDownCache!.TotalFuel,
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

      this.RunVM.LandingCache = new(DateTime.Now, this.simPropValues.TotalFuelKg, this.simPropValues.IAS,
        this.simPropValues.TouchdownBankDegrees, this.simPropValues.TouchdownLatitude, this.simPropValues.TouchdownLongitude,
        this.simPropValues.TouchdownVelocity, this.simPropValues.TouchdownPitchDegrees,
        this.simPropValues.Latitude, this.simPropValues.Longitude);
      this.RunVM.State = RunViewModel.RunModelState.LandedWaitingForShutdown;
    }

    private void ProcessWaitForTakeOff()
    {
      if (!this.simPropValues.IsFlying) return;

      this.RunVM.TakeOffCache = new(DateTime.Now, this.simPropValues.TotalFuelKg, this.simPropValues.IAS, this.simPropValues.Latitude, this.simPropValues.Longitude);
      UpdateSimbriefAndVatsimIfRequired();

      this.RunVM.State = RunViewModel.RunModelState.InFlightWaitingForLanding;
    }

    private void ProcessWaitForOffBlocks()
    {
      if (this.simPropValues.ParkingBrakeSet) return;

      if (RunVM.State == RunViewModel.RunModelState.AfterShutdown)
        this.RunVM.Clear();

      RunVM.StartUpCache = new(DateTime.Now, simPropValues.TotalFuelKg, simPropValues.Latitude, simPropValues.Longitude);
      UpdateSimbriefAndVatsimIfRequired();

      RunVM.State = RunViewModel.RunModelState.StartedWaitingForTakeOff;
    }

    private void UpdateSimbriefAndVatsimIfRequired()
    {
      //TODO do both in async:
      if (this.RunVM.VatsimCache == null && this.settings.VatsimId != null)
        RunVM.VatsimCache = VatsimProvider.CreateData(this.settings.VatsimId);
      if (this.RunVM.SimDataCache == null && this.settings.SimBriefId != null)
        RunVM.SimDataCache = SimBriefProvider.CreateData(this.settings.SimBriefId);
    }

    internal void Start()
    {
      this.simObj.ExtTime.SimSecondElapsed += () => CheckForNextState();
    }
  }
}
