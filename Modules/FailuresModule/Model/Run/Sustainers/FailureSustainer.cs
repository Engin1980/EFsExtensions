using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using ESimConnect;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.Run.Sustainers
{
  public abstract class FailureSustainer : NotifyPropertyChangedBase
  {
    public FailureDefinition Failure { get; }

    public bool IsActive
    {
      get => base.GetProperty<bool>(nameof(IsActive))!;
      private set => base.UpdateProperty(nameof(IsActive), value);
    }
    private static ESimConnect.ESimConnect simCon = null!;
    protected ESimConnect.ESimConnect SimCon { get => FailureSustainer.simCon; }

    public static void InitSimCon(ESimConnect.ESimConnect simCon)
    {
      FailureSustainer.simCon = simCon ?? throw new ArgumentNullException(nameof(simCon));
    }

    protected FailureSustainer(FailureDefinition failure)
    {
      this.Failure = failure ?? throw new ArgumentNullException(nameof(failure));
    }

    public void Start()
    {
      if (SimCon == null) throw new ApplicationException("SimCon is null.");
      if (!IsActive)
      {
        this.StartInternal();
        this.IsActive = true;
      }
    }

    public void Reset()
    {
      if (SimCon == null) throw new ApplicationException("SimCon is null.");
      if (IsActive)
      {
        this.ResetInternal();
        this.IsActive = false;
      }
    }

    public void Toggle()
    {
      if (!IsActive)
        Start();
      else
        Reset();
    }

    protected abstract void StartInternal();
    protected abstract void ResetInternal();
  }
}
