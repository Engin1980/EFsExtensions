using ESystem.Asserting;
using System;

namespace FailuresModule.Model.Sim
{
  //TODO remove types and replace by enum if not used in more complex way
  public class StuckFailureDefinition : FailureDefinition
  {
    #region Private Fields

    private const bool DEFAULT_ONLY_UPDATE_ON_DETECTED_CHANGE = false;
    private const int DEFAULT_REFRESH_INTERVAL_IN_MS = 1000;

    #endregion Private Fields

    #region Public Properties

    public bool OnlyUpdateOnDetectedChange { get; set; } = DEFAULT_ONLY_UPDATE_ON_DETECTED_CHANGE;
    public int RefreshIntervalInMs { get; set; } = DEFAULT_REFRESH_INTERVAL_IN_MS;
    public override string Type => "Stuck";

    #endregion Public Properties

    #region Public Constructors

    public StuckFailureDefinition(string id, string title, string simConPoint) : base(id, title, simConPoint)
    {
    }

    internal void EnsureValid()
    {
      EAssert.IsTrue(RefreshIntervalInMs > 50);
    }

    #endregion Public Constructors
  }
}
