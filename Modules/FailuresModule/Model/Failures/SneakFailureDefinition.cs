using ChlaotModuleBase;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace FailuresModule.Model.Failures
{
  public class SneakFailureDefinition : WithSimVarFailureDefinition
  {
    #region Public Enums

    public enum EDirection
    {
      Up,
      Down
    }

    #endregion Public Enums

    #region Public Fields

    public const int DEFAULT_TICK_INTERVAL_IN_MS = 1000;

    #endregion Public Fields

    #region Public Properties

    public EDirection Direction { get; set; }
    public string FinalFailureId { get; set; } = string.Empty;
    public double FinalValue { get; set; } = double.NaN;
    public double MaximalInitialSneakValue { get; set; } = double.NaN;
    public double MaximalSneakAdjustPerSecond { get; set; } = double.NaN;
    public double MinimalInitialSneakValue { get; set; } = double.NaN;
    public double MinimalSneakAdjustPerSecond { get; set; } = double.NaN;
    public double TickIntervalInMS { get; set; } = DEFAULT_TICK_INTERVAL_IN_MS;
    public bool IsPercentageBased { get; set; }
    public override string Type => "Sneak";
    public override string SimConPoint => SimVar;

    #endregion Public Properties

    #region Public Constructors

    #endregion Public Constructors

    #region Public Methods
    public override void PostDeserialize()
    {
      base.PostDeserialize();
      EAssert.IsTrue(!double.IsNaN(MaximalInitialSneakValue));
      EAssert.IsTrue(!double.IsNaN(MaximalSneakAdjustPerSecond));
      EAssert.IsTrue(!double.IsNaN(MinimalInitialSneakValue));
      EAssert.IsTrue(!double.IsNaN(MinimalSneakAdjustPerSecond));
      EAssert.IsTrue(!double.IsNaN(FinalValue));
      EAssert.IsTrue(MinimalInitialSneakValue <= MaximalInitialSneakValue);
      EAssert.IsTrue(MinimalSneakAdjustPerSecond <= MaximalSneakAdjustPerSecond);
      EAssert.IsNonEmptyString(this.FinalFailureId);
      EAssert.IsTrue(this.TickIntervalInMS > 50);

    }

    internal override void ExpandVariableIfExists(string varRef, int variableValue)
    {
      base.ExpandVariableIfExists(varRef, variableValue);
      if (this.FinalFailureId.Contains(varRef))
        this.FinalFailureId = this.FinalFailureId.Replace(varRef, variableValue.ToString());
    }

    #endregion Public Methods
  }
}
