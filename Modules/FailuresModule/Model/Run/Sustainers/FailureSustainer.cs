using Eng.Chlaot.ChlaotModuleBase;
using ESimConnect;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Run.Sustainers
{
  public abstract class FailureSustainer : NotifyPropertyChangedBase
  {
    #region Fields

    private static ESimConnect.ESimConnect simCon = null!;
    private bool initialized = false;

    #endregion Fields

    #region Properties

    public FailureDefinition Failure { get; }

    public bool IsActive
    {
      get => base.GetProperty<bool>(nameof(IsActive))!;
      private set => base.UpdateProperty(nameof(IsActive), value);
    }
    protected ESimConnect.ESimConnect SimCon { get => simCon; }

    #endregion Properties

    #region Constructors

    protected FailureSustainer(FailureDefinition failure)
    {
      this.Failure = failure ?? throw new ArgumentNullException(nameof(failure));
    }

    #endregion Constructors

    #region Methods

    public void Reset()
    {
      if (SimCon == null) throw new ApplicationException("SimCon is null.");
      if (IsActive)
      {
        this.ResetInternal();
        this.IsActive = false;
      }
    }

    public void Start()
    {
      if (SimCon == null) throw new ApplicationException("SimCon is null.");
      if (!initialized)
        Init();

      if (!IsActive)
      {
        this.StartInternal();
        this.IsActive = true;
      }
    }

    public void Toggle()
    {
      if (!IsActive)
        Start();
      else
        Reset();
    }

    internal static void SetSimCon(ESimConnect.ESimConnect eSimConnect)
    {
      FailureSustainer.simCon = eSimConnect;
    }

    protected abstract void InitInternal();

    protected abstract void ResetInternal();

    protected abstract void StartInternal();

    private void Init()
    {
      InitInternal();
      initialized = true;
    }

    #endregion Methods
  }
}
