using Eng.EFsExtensions.Libs.AirportsLib;
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

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class RunContext : NotifyPropertyChanged
  {
    private class SimPropValues
    {
      private const int EMPTY_TYPE_ID = -1;
      private readonly ESimConnect.Extenders.ValueCacheExtender cache;

      private TypeId[] engRunningTypeId = new TypeId[] { new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID) };
      private TypeId[] wheelOnGroundTypeId = new TypeId[] { new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID) };

      private TypeId parkingBrakeTypeId = new(EMPTY_TYPE_ID);
      private TypeId heightTypeId = new(EMPTY_TYPE_ID);
      private TypeId latitudeTypeId = new(EMPTY_TYPE_ID);
      private TypeId longitudeTypeId = new(EMPTY_TYPE_ID);
      private TypeId iasTypeId = new(EMPTY_TYPE_ID);
      private TypeId fuelQuantityKgTypeId = new(EMPTY_TYPE_ID);
      private TypeId touchdownBankDegrees = new(EMPTY_TYPE_ID);
      private TypeId touchdownLatitude = new (EMPTY_TYPE_ID);
      private TypeId touchdownLongitude = new (EMPTY_TYPE_ID);
      private TypeId touchdownPitchDegrees = new (EMPTY_TYPE_ID);
      private TypeId touchdownVelocity = new (EMPTY_TYPE_ID);

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

    private readonly SimPropValues simPropValues = null!;
    private readonly Settings settings = null!;
    private readonly ESimConnect.Extenders.ValueCacheExtender cache;

    public RunContext(InitContext initContext, ESimConnect.Extenders.ValueCacheExtender cache, Settings settings)
    {
      EAssert.Argument.IsNotNull(initContext, nameof(initContext));
      EAssert.IsNotNull(initContext.SelectedProfile, "SelectedProfile not set.");
      EAssert.Argument.IsNotNull(cache, nameof(cache));

      this.RunVM.Profile = initContext.SelectedProfile;
      this.cache = cache;
      this.settings = settings;
    }

    internal RunViewModel RunVM
    {
      get { return base.GetProperty<RunViewModel>(nameof(RunVM))!; }
      set { base.UpdateProperty(nameof(RunVM), value); }
    }

    public void ProcessSecondElapsed()
    {
      CheckForNextState();
    }

    private void CheckForNextState()
    {
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

      this.RunVM.ShutDownCache = new(DateTime.Now);
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

      //TODO both do in async
      if (this.RunVM.VatsimCache == null && this.settings.VatsimId != null)
        RunVM.VatsimCache = VatsimProvider.CreateData(this.settings.VatsimId);
      if (this.RunVM.SimDataCache == null && this.settings.SimBriefId != null)
        RunVM.SimDataCache = SimBriefProvider.CreateData(this.settings.SimBriefId);

      this.RunVM.State = RunViewModel.RunModelState.InFlightWaitingForLanding;
    }

    private void ProcessWaitForOffBlocks()
    {
      if (this.simPropValues.ParkingBrakeSet) return;

      RunVM.StartUpCache = new(DateTime.Now, simPropValues.TotalFuelKg, simPropValues.Latitude, simPropValues.Longitude);

      //TODO do both in async:
      if (this.settings.VatsimId != null)
        RunVM.VatsimCache = VatsimProvider.CreateData(this.settings.VatsimId);
      else
        RunVM.VatsimCache = null;

      if (this.settings.SimBriefId != null)
        RunVM.SimDataCache = SimBriefProvider.CreateData(this.settings.SimBriefId);
      else
        RunVM.SimDataCache = null;

      RunVM.State = RunViewModel.RunModelState.StartedWaitingForTakeOff;
    }
  }
}
