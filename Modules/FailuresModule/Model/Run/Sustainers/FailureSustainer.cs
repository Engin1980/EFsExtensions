using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.Run.Sustainers
{
    public abstract class FailureSustainer
  {
    public FailureDefinition Failure { get; }
    public bool Initialized { get; private set; }

    protected FailureSustainer(FailureDefinition failure)
    {
      Failure = failure ?? throw new ArgumentNullException(nameof(failure));
    }

    public void Init()
    {
      this.InitInternal();
      this.Initialized = true;
    }

    public void Tick(SimData simData)
    {
      if (!Initialized) throw new ApplicationException("FailureSustainer not initialized.");
      this.TickInternal(simData);
    }

    protected abstract void InitInternal();
    protected abstract void TickInternal(SimData simData);
  }
}
