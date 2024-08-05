using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.Modules.FlightLogModule;
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

namespace FlightLogModule
{
  /// <summary>
  /// Interaction logic for CtrRun.xaml
  /// </summary>
  public partial class CtrRun : UserControl
  {
    private readonly Context Context = null!;
    private readonly SimObject simObject = SimObject.GetInstance();
    private static class SimProperties
    {
      public static readonly SimProperty Eng1Running = new("Eng1Running", ESimConnect.Definitions.SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "1", null);
      public static readonly SimProperty Height = new("Height", ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, null);
      public static readonly SimProperty Latitude = new("Latitude", ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE, null);
      public static readonly SimProperty Longitude = new("Longitude", ESimConnect.Definitions.SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE, null);
    }

    public CtrRun()
    {
      InitializeComponent();
    }

    public CtrRun(Context context) : this()
    {
      this.Context = context;
      this.Context.RunModel = new RunModel.RunModel();

      this.DataContext = this.Context;

      this.simObject.SimSecondElapsed += SimObject_SimSecondElapsed;
    }

    private void SimObject_SimSecondElapsed()
    {
      CheckForNextState();
    }

    private void CheckForNextState()
    {
      switch (Context.RunModel.State)
      {
        case RunModel.RunModel.RunModelState.WaitingForStartup:
          ProcessWaitForStartupSecondElapsed();
          break;
        case RunModel.RunModel.RunModelState.StartedWaitingForTakeOff:
          ProcessStartedWaitingForTakeOffSecondElapsed();
          break;
        case RunModel.RunModel.RunModelState.InFlightWaitingForLanding:
          InFlightWaitingForLandingSecondElapsed();
          break;
        case RunModel.RunModel.RunModelState.LandedWaitingForShutdown:
          LandedWaitingForShutdown();
          break;
        default:
          throw new ESystem.Exceptions.UnexpectedEnumValueException(Context.RunModel.State);
      }
    }

    private void LandedWaitingForShutdown()
    {
      if (this.simObject[SimProperties.Eng1Running] == 1) return;

      this.Context.RunModel.ShutDownCache = new(DateTime.Now);
    }

    private void InFlightWaitingForLandingSecondElapsed()
    {
      throw new NotImplementedException();
      // tohle vůbec nevím co bude dělat :-D
    }

    private int airborneCounter = 0;
    private const int AIRBORNE_COUNTER_THRESHOLD = 30;

    private void ProcessStartedWaitingForTakeOffSecondElapsed()
    {
      if (this.simObject[SimProperties.Height] == 0)
      {
        if (airborneCounter > 0)
        {
          airborneCounter = 0;
          Context.RunModel.TakeOffCache = null;
        }
        return;
      }
      else
      {
        if (airborneCounter == 0)
          Context.RunModel.TakeOffCache = new(DateTime.Now, this.simObject[SimProperties.Latitude], this.simObject[SimProperties.Longitude]);
        airborneCounter++;
      }

      if (airborneCounter > AIRBORNE_COUNTER_THRESHOLD)
      {
        airborneCounter = 0;
        Context.RunModel.State = RunModel.RunModel.RunModelState.InFlightWaitingForLanding;
      }
    }

    private void ProcessWaitForStartupSecondElapsed()
    {
      if (this.simObject[SimProperties.Eng1Running] == 0) return;

      Context.RunModel.StartUpCache = new(DateTime.Now);
      Context.RunModel.State = RunModel.RunModel.RunModelState.StartedWaitingForTakeOff;
    }
  }
}
