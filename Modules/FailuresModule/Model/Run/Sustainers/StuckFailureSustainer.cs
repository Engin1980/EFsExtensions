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
    private uint stuckValue = 0;

    public StuckFailureSustainer(StuckFailureDefinition failure) : base(failure)
    {
    }

    protected override void ResetInternal()
    {
      // intentionally blank
    }

    protected override void StartInternal()
    {
      this.SimCon.DataReceived += SimCon_DataReceived;
      // this.SimCon.RequestData(...);
      throw new NotImplementedException();
    }

    private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      // get data and store to stuck-value
      throw new NotImplementedException();
    }

    protected override void TickInternal(SimData simData)
    {
      // set stuck-value
      throw new NotImplementedException();
    }
  }
}
