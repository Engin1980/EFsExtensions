using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.RunVM.Sustainers
{
  internal class StuckFailureSustainer : FailureSustainer
  {
    public StuckFailureSustainer(FailureDefinition failure) : base(failure)
    {
    }

    protected override void InitInternal()
    {
      throw new NotImplementedException();
    }

    internal void Tick(Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection.SimData simData)
    {
      throw new NotImplementedException();
    }
  }
}
