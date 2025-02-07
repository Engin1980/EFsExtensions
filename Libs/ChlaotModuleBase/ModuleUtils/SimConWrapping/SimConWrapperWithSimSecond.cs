using ChlaotModuleBase.ModuleUtils.SimConWrapping;
using ELogging;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping
{
  public class SimConWrapperWithSimSecond : SimConWrapperWithOpenAsync
  {
    #region Public Delegates

    public delegate void SimSecondElapsedDelegate();

    #endregion Public Delegates

    #region Public Events

    public event SimSecondElapsedDelegate? SimSecondElapsed;

    #endregion Public Events

    #region Private Fields


    #endregion Private Fields

    #region Public Properties

    public bool IsSimPaused { get; private set; } = false;

    #endregion Public Properties

    #region Public Constructors

    public SimConWrapperWithSimSecond(ESimConnect.ESimConnect simCon) : base(simCon) { }

    #endregion Public Constructors

    #region Public Methods

    protected override void StartProtected()
    {
      base.StartProtected();

      simCon.SystemEventInvoked += SimCon_EventInvoked;
      simCon.SystemEvents.Register(ESimConnect.Definitions.SimEvents.System.Pause);
      simCon.SystemEvents.Register(ESimConnect.Definitions.SimEvents.System._1sec);
    }

    #endregion Public Methods

    #region Private Methods

    private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectSystemEventInvokedEventArgs e)
    {
      //TODO duplicit with SimSecondElapsedExtender
      if (e.Event == ESimConnect.Definitions.SimEvents.System.Pause)
      {
        IsSimPaused = e.Value != 0;
      }
      else if (e.Event == ESimConnect.Definitions.SimEvents.System._1sec)
      {
        SimSecondElapsed?.Invoke();
      }
    }

    #endregion Private Methods

  }
}
