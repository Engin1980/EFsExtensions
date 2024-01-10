using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Sim
{
  internal class SimVarViaEventFailureDefinition : FailureDefinition
  {
    #region Public Properties

    public bool OnlyUpdateOnDetectedChange { get; set; } = StuckFailureDefinition.DEFAULT_ONLY_UPDATE_ON_DETECTED_CHANGE;

    public double FailValue { get; set; } = SimVarFailureDefinition.DEFAULT_FAIL_VALUE;

    public int RefreshIntervalInMs { get; set; } = StuckFailureDefinition.DEFAULT_REFRESH_INTERVAL_IN_MS;

    public string SimEventConPoint { get; set; }

    public string SimVarConPoint { get; set; }

    public override string Type => "StuckViaEvent";

    #endregion Public Properties

    #region Public Constructors

    public SimVarViaEventFailureDefinition(string id, string title, string simVarConPoint, string simEventConPoint) : base(id, title, simVarConPoint + "/@" + simEventConPoint)
    {
      this.SimVarConPoint = simVarConPoint;
      this.SimEventConPoint = simEventConPoint;
    }

    #endregion Public Constructors

    #region Internal Methods

    internal override void ExpandVariableIfExists(string varRef, int variableValue)
    {
      base.ExpandVariableIfExists(varRef, variableValue);
      SimVarConPoint = ExpandVariable(SimVarConPoint, varRef, variableValue);
      SimEventConPoint = ExpandVariable(SimEventConPoint, varRef, variableValue);
    }

    #endregion Internal Methods

    #region Private Methods

    private string ExpandVariable(string txt, string varRef, int variableValue)
    {
      string ret;
      if (txt.Contains(varRef))
      {
        ret = new StringBuilder(txt).Replace(varRef, variableValue.ToString()).ToString();
      }
      else
        ret = txt;
      return ret;
    }

    #endregion Private Methods
  }
}
