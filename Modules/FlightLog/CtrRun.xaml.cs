using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using ESimConnect;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  /// <summary>
  /// Interaction logic for CtrRun.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private class SimPropValues
    {
      private const int EMPTY_TYPE_ID = -1;
      private TypeId[] engRunningTypeId = new TypeId[] { new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID), new(EMPTY_TYPE_ID) };
      private readonly ESimConnect.Extenders.ValueCacheExtender cache;
      private TypeId parkingBrakeTypeId = new(EMPTY_TYPE_ID);
      private TypeId heightTypeId = new(EMPTY_TYPE_ID);
      private TypeId latitudeTypeId = new(EMPTY_TYPE_ID);
      private TypeId longitudeTypeId = new(EMPTY_TYPE_ID);
      private TypeId iasTypeId = new(EMPTY_TYPE_ID);

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
        this.iasTypeId = cache.Register(
          ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED,
          ESimConnect.Definitions.SimUnits.Speed.KNOT);
      }

      public bool ParkingBrakeSet => cache.GetValue(parkingBrakeTypeId) == 1;
      public double Height => cache.GetValue(heightTypeId);
      public double Latitude => cache.GetValue(latitudeTypeId);
      public double Longitude => cache.GetValue(longitudeTypeId);
      public bool IsAnyEngineRunning => engRunningTypeId.Any(q => cache.GetValue(q) == 1);
      public double IAS => cache.GetValue(iasTypeId);
      public bool IsFlying => Height > 20 && IAS > 40;

    }

    private readonly Context Context = null!;
    private readonly NewSimObject simObject = null!;
    private readonly SimPropValues simPropValues = null!;


    public CtrRun()
    {
      InitializeComponent();
    }

    public CtrRun(Context context) : this()
    {
      this.simObject = NewSimObject.GetInstance();
      this.simPropValues = new SimPropValues(this.simObject.ExtValue);

      this.Context = context;
      this.Context.RunModel = new RunModel();

      this.DataContext = this.Context;

      this.simObject.ExtTime.SimSecondElapsed += SimObject_SimSecondElapsed;
    }

    private void SimObject_SimSecondElapsed()
    {
      CheckForNextState();
    }

    private void CheckForNextState()
    {
      switch (Context.RunModel.State)
      {
        case RunModel.RunModelState.WaitingForStartup:
          ProcessWaitForStartupSecondElapsed();
          break;
        case RunModel.RunModelState.StartedWaitingForTakeOff:
          ProcessStartedWaitingForTakeOffSecondElapsed();
          break;
        case RunModel.RunModelState.InFlightWaitingForLanding:
          InFlightWaitingForLandingSecondElapsed();
          break;
        case RunModel.RunModelState.LandedWaitingForShutdown:
          LandedWaitingForShutdown();
          break;
        default:
          throw new ESystem.Exceptions.UnexpectedEnumValueException(Context.RunModel.State);
      }
    }

    private void LandedWaitingForShutdown()
    {
      if (!this.simPropValues.ParkingBrakeSet) return;
      if (this.simPropValues.IsAnyEngineRunning) return;

      this.Context.RunModel.ShutDownCache = new(DateTime.Now);
    }

    private void InFlightWaitingForLandingSecondElapsed()
    {
      throw new NotImplementedException();
      // tohle vůbec nevím co bude dělat :-D
    }

    private void ProcessStartedWaitingForTakeOffSecondElapsed()
    {
      if (!this.simPropValues.IsFlying) return;

      this.Context.RunModel.TakeOffCache = new(DateTime.Now, this.simPropValues.Latitude, this.simPropValues.Longitude);
      this.Context.RunModel.State = RunModel.RunModelState.InFlightWaitingForLanding;
    }

    private void ProcessWaitForStartupSecondElapsed()
    {
      if (this.simPropValues.ParkingBrakeSet) return;

      Context.RunModel.StartUpCache = new(DateTime.Now);
      Context.RunModel.State = RunModel.RunModelState.StartedWaitingForTakeOff;
    }
  }
}
