using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Model.Sim;

namespace FailuresModule.Types.Run.Sustainers
{
  internal class StuckFailureSustainer : FailureSustainer
  {
    private bool isRegistered = false;
    private double? stuckValue = null;
    private int typeId = -1;
    private int requestId = -1;

    public StuckFailureSustainer(StuckFailureDefinition failure) : base(failure)
    {
    }

    protected override void ResetInternal()
    {
      this.stuckValue = null;
    }

    protected override void StartInternal()
    {
      if (!isRegistered)
        Register();

      this.SimCon.RequestPrimitive(this.typeId, out this.requestId);
    }

    private const string DEFAULT_UNIT = "Number"; // taken from https://github.com/kanaron/RandFailuresFS2020/blob/ab2cb278df8ede6739bcfe60a7f34c9f97b8f5ba/RandFailuresFS2020/RandFailuresFS2020/Simcon.cs
    private const string DEFAULT_TYPE = "FLOAT64";

    private void Register()
    {
      this.SimCon.DataReceived += SimCon_DataReceived;

      string name = this.Failure.SimConPoint.SimPointName;
      this.typeId = this.SimCon.RegisterPrimitive<double>(name, DEFAULT_UNIT, DEFAULT_TYPE);
      this.isRegistered = true;
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      if (e.RequestId == this.requestId)
      {
        double val = (double)e.Data;
        this.stuckValue = val;
      }
    }

    protected override void TickInternal(SimData simData)
    {
      if (stuckValue == null) return;
      // set stuck-value
      throw new NotImplementedException();
      /*
       * sc.SetDataOnSimObject(eDef, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dValue);
       * https://github.com/kanaron/RandFailuresFS2020/blob/ab2cb278df8ede6739bcfe60a7f34c9f97b8f5ba/RandFailuresFS2020/RandFailuresFS2020/Simcon.cs
       * line 165
       * 
       * */
    }
  }
}
