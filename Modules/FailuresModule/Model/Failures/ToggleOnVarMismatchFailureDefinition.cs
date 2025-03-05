using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Failures
{
  internal class ToggleOnVarMismatchFailureDefinition : WithSimVarFailureDefinition
  {
    #region Public Properties

    public double FailValue { get; set; } = SetFailureDefinition.DEFAULT_FAIL_VALUE;
    public bool OnlyUpdateOnDetectedChange { get; set; } = StuckFailureDefinition.DEFAULT_ONLY_UPDATE_ON_DETECTED_CHANGE;
    public int RefreshIntervalInMs { get; set; } = StuckFailureDefinition.DEFAULT_REFRESH_INTERVAL_IN_MS;

    public override string SimConPoint => $"{SimVar}/@{SimEvent}";
    public string SimEvent { get; set; } = null!;

    public override string Type => "Toggle on var-miss";
    #endregion Public Properties

    #region Public Constructors

    #endregion Public Constructors

    #region Methods

    public override void PostDeserialize()
    {
      base.PostDeserialize();
      EAssert.IsNonEmptyString(SimVar);
      EAssert.IsNonEmptyString(SimEvent);
    }

    #endregion Methods

    #region Internal Methods

    internal override void ExpandVariableIfExists(string varRef, int variableValue)
    {
      base.ExpandVariableIfExists(varRef, variableValue);
      SimEvent = ExpandVariableInString(SimEvent, varRef, variableValue);
    }

    #endregion Internal Methods

    #region Private Methods

    #endregion Private Methods
  }
}
