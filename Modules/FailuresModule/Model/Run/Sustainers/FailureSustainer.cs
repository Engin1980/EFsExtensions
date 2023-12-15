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
    public bool IsActive { get; private set; }
    protected ESimConnect.ESimConnect SimCon { get; private set; }

    protected FailureSustainer(FailureDefinition failure)
    {
      Failure = failure ?? throw new ArgumentNullException(nameof(failure));
    }

    public void Start()
    {
      if (!IsActive)
      {
        this.StartInternal();
        this.IsActive = true;
      }
    }

    public void Reset()
    {
      if (IsActive)
      {
        this.ResetInternal();
        this.IsActive = false;
      }
    }

    public void Tick(SimData simData)
    {
      if (IsActive)
        this.TickInternal(simData);
    }

    protected abstract void StartInternal();
    protected abstract void ResetInternal();
    protected abstract void TickInternal(SimData simData);
  }
}
