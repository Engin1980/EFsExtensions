using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.Run.Sustainers
{
    internal class EventFailureSustainer : FailureSustainer
  {
    public EventFailureSustainer(EventFailureDefinition failure) : base(failure)
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
