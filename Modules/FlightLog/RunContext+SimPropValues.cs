using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public partial class RunContext
  {
    private class SimPropValues
    {
      private const int EMPTY_TYPE_ID = -1;
      private readonly ESimConnect.Extenders.ValueCacheExtender cache;

      private readonly TypeId[] engRunningTypeId = new TypeId[] { new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID) };
      private readonly TypeId simOnGroundTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId parkingBrakeTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId heightTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId latitudeTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId longitudeTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId iasTypeId = new(EMPTY_TYPE_ID);
      private readonly TypeId fuelQuantityLtrsTypeId = new(EMPTY_TYPE_ID);
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
        this.latitudeTypeId = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE, ESimConnect.Definitions.SimUnits.Angle.DEGREE);
        this.longitudeTypeId = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE, ESimConnect.Definitions.SimUnits.Angle.DEGREE);

        this.engRunningTypeId[0] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "1");
        this.engRunningTypeId[1] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "2");
        this.engRunningTypeId[2] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "3");
        this.engRunningTypeId[3] = cache.Register(ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "4");

        this.simOnGroundTypeId = cache.Register("SIM ON GROUND");

        this.iasTypeId = cache.Register(
          ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED,
          ESimConnect.Definitions.SimUnits.Speed.KNOT);
        this.fuelQuantityLtrsTypeId = cache.Register("FUEL TOTAL QUANTITY", ESimConnect.Definitions.SimUnits.Volume.LITER); // weights Kgs not working

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
      public double IAS => cache.GetValue(iasTypeId);
      public bool IsFlying => cache.GetValue(simOnGroundTypeId) == 0;

      public double TotalFuelLtrs => cache.GetValue(fuelQuantityLtrsTypeId);

      public int EmptyWeightKg => (int)cache.GetValue(emptyWeightKgTypeId);
      public int TotalWeightKg => (int)cache.GetValue(totalWeightKgTypeId);
    }
  }
}
