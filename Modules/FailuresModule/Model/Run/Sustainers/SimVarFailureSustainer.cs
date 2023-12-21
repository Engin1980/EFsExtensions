using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Model.Run.Sustainers;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.Run.Sustainers
{
  internal class SimVarFailureSustainer : SimVarBasedFailureSustainer
  {
    private const uint OK = 0; // from flightsimulator API
    private const uint FAILED = 1;
    public SimVarFailureSustainer(SimVarFailureDefinition failure) : base(failure)
    {
    }

    protected override void ResetInternal()
    {
      SendEvent(OK);
    }

    protected override void StartInternal()
    {
      SendEvent(FAILED);
    }

    private void SendEvent(uint arg)
    {
      base.SendData(arg);
    }
  }
}
