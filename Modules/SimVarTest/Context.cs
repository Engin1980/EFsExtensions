using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using ESystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static ESystem.Functions;

namespace Eng.Chlaot.Modules.SimVarTestModule
{
  public class Context : NotifyPropertyChangedBase
  {
    private readonly Action onReadySet;
    private ESimConnect.ESimConnect simCon = null!;
    private Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping.SimConWrapperWithSimSecond simConWrapper = null!;
    private record SimVarId(int TypeId, int RequestId, SimVarCase Case);
    private readonly List<SimVarId> SimVarIds = new();

    public BindingList<SimVarCase> Cases { get; } = new();

    public bool? IsEnabled
    {
      get => base.GetProperty<bool?>(nameof(IsEnabled))!;
      set
      {
        base.UpdateProperty(nameof(IsEnabled), value);
        onReadySet();
      }
    }

    public Context(Action onReadySet)
    {
      this.onReadySet = onReadySet;
    }

    public void Connect()
    {
      simCon = new ESimConnect.ESimConnect();
      simConWrapper = new(simCon);
      simConWrapper.OpenAsync(() => { }, ex => { });

      simCon.DataReceived += SimCon_DataReceived;
      simCon.ThrowsException += SimCon_ThrowsException;
    }

    private void SimCon_ThrowsException(ESimConnect.ESimConnect sender, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_EXCEPTION ex)
    {
      Logger.Log(this, LogLevel.ERROR, $"SimCon throws error - {ex}");
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      SimVarId? sid = SimVarIds.FirstOrDefault(q => q.RequestId == e.RequestId);
      if (sid == null) // probably deleted one
        return;

      SimVarCase svc = sid.Case;
      svc.Value = (double)e.Data;
    }

    internal void RegisterNewSimVar(string name, bool validateName)
    {
      int typeId;
      try
      {
        typeId = simCon.RegisterPrimitive<double>(name, validate: validateName);
      }
      catch (Exception ex)
      {
        Logger.Log(this, LogLevel.ERROR, $"Unable register '{name}'. {ex.Message}");
        return;
      }

      simCon.RequestPrimitiveRepeatedly(typeId, out int requestId, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_PERIOD.SECOND, true, 0, 0, 0);

      SimVarCase svc = new()
      {
        SimVar = name,
        Value = Double.NaN
      };
      SimVarId sid = new(typeId, requestId, svc);
      SimVarIds.Add(sid);
      this.Cases.Add(svc);

    }

    internal void SetValue(SimVarCase simVarCase, double newValue)
    {
      SimVarId sid = SimVarIds.First(q => q.Case == simVarCase);
      simCon.SendPrimitive<double>(sid.TypeId, newValue);
    }

    internal void DeleteSimVar(SimVarCase svc)
    {
      SimVarId sid = SimVarIds.First(q=>q.Case == svc);
      SimVarIds.Remove(sid);
    }
  }
}
