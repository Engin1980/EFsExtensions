using ESystem.Asserting;
using EXmlLib.Interfaces;

namespace FailuresModule.Model.Failures
{
  public class SetFailureDefinition : WithSimVarFailureDefinition
  {
    #region Private Fields

    public const int DEFAULT_FAIL_VALUE = 1;
    public const int DEFAULT_OK_VALUE = 0;

    #endregion Private Fields

    #region Public Properties

    public double FailValue { get; set; } = DEFAULT_FAIL_VALUE;
    public double OkValue { get; set; } = DEFAULT_OK_VALUE;
    public override string SimConPoint => SimVar;
    public override string Type => "Set";

    #endregion Public Properties

  }
}
