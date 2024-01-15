using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConExtenders
{
  public class SimSecondElapsedExtender
  {
    private readonly ESimConnect.ESimConnect simCon;
    public bool IsSimPaused { get; set; }
    public event Action? SimSecondElapsed;
    private readonly bool invokeSimSecondEventsOnPause;

    public SimSecondElapsedExtender(ESimConnect.ESimConnect simCon, bool invokeSimSecondEventsOnPause)
    {
      EAssert.Argument.IsNotNull(simCon, nameof(simCon));
      this.simCon = simCon;
      this.simCon.EventInvoked += SimCon_EventInvoked;
      this.invokeSimSecondEventsOnPause = invokeSimSecondEventsOnPause;
      if (simCon.IsOpened) RegisterEvents();
      else
        simCon.Connected += _ => RegisterEvents();
    }

    private void RegisterEvents()
    {
      simCon.RegisterSystemEvent(ESimConnect.SimEvents.System.Pause);
      simCon.RegisterSystemEvent(ESimConnect.SimEvents.System._1sec);
    }

    private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
    {
      if (e.Event == ESimConnect.SimEvents.System.Pause)
      {
        IsSimPaused = e.Value != 0;
      }
      else if (e.Event == ESimConnect.SimEvents.System._1sec && (!IsSimPaused || invokeSimSecondEventsOnPause))
      {
        SimSecondElapsed?.Invoke();
      }
    }
  }
}
