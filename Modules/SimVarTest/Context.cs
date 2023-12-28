using Eng.Chlaot.ChlaotModuleBase;
using SimVarTestModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      SimVarId sid = SimVarIds.First(q => q.RequestId == e.RequestId);
      SimVarCase svc = sid.Case;
      svc.Value = (double) e.Data;
    }

    private const int CLIENT_DATA_ID = 1;

    internal void RegisterNewSimVar(string name)
    {
      SimVarCase svc = new()
      {
        SimVar = name,
        Value = Double.NaN
      };

      this.Cases.Add(svc);
      int typeId = simCon.RegisterCustomPrimitive<double>(name, CLIENT_DATA_ID);

      simCon.RequestPrimitiveRepeatedly(typeId, out int requestId, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_PERIOD.SECOND, true, 0, 0, 0);
      SimVarId sid = new(typeId, requestId, svc);
      SimVarIds.Add(sid);
    }
  }
}
