namespace FailuresModule.Model.Sim
{
  public class SimVarFailureDefinition : FailureDefinition
  {
    #region Private Fields

    private const int DEFAULT_FAIL_VALUE = 1;
    private const int DEFAULT_OK_VALUE = 0;

    #endregion Private Fields

    #region Public Properties

    public double FailValue { get; set; } = DEFAULT_FAIL_VALUE;
    public double OkValue { get; set; } = DEFAULT_OK_VALUE;
    public override string Type => "SimVar";

    #endregion Public Properties

    #region Public Constructors

    public SimVarFailureDefinition(string id, string title, string simConPoint) : base(id, title, simConPoint)
    {
    }

    #endregion Public Constructors
  }
}
