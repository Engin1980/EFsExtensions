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
    public StuckFailureSustainer(StuckFailureDefinition failure) : base(failure)
    {
    }

    protected override void InitInternal()
    {
      throw new NotImplementedException();
    }

    protected override void TickInternal(SimData simData)
    {
      throw new NotImplementedException();
    }
  }
}
