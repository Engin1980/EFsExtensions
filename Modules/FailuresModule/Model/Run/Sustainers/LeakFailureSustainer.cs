using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Model.Sim;

namespace FailuresModule.Types.Run.Sustainers
{
  internal class LeakFailureSustainer : FailureSustainer
  {
    private uint leakPerTick = 0;
    private uint currentValue = 0;

    public LeakFailureSustainer(LeakFailureDefinition failure) : base(failure)
    {
    }

    protected override void ResetInternal()
    {
      leakPerTick = 0;
      currentValue = 0;
    }

    protected override void StartInternal()
    {
      // connect and get current value
      throw new NotImplementedException();
    }

    protected override void TickInternal(SimData simData)
    {
      // lower the value and set it to sim
      throw new NotImplementedException();
    }
  }
}
