using ESystem.Asserting;
using System;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Failures
{
  public class StuckFailureDefinition : WithSimVarFailureDefinition
  {
    #region Private Fields

    public const bool DEFAULT_ONLY_UPDATE_ON_DETECTED_CHANGE = false;
    public const int DEFAULT_REFRESH_INTERVAL_IN_MS = 1000;

    #endregion Private Fields

    #region Public Properties

    public bool OnlyUpdateOnDetectedChange { get; set; } = DEFAULT_ONLY_UPDATE_ON_DETECTED_CHANGE;
    public int RefreshIntervalInMs { get; set; } = DEFAULT_REFRESH_INTERVAL_IN_MS;
    public override string Type => "Stuck";
    public override string SimConPoint => SimVar;

    #endregion Public Properties

    #region Public Constructors

    public override void PostDeserialize()
    {
      base.PostDeserialize();
      EAssert.IsTrue(RefreshIntervalInMs > 50, $"Invalid refresh interval ({this.RefreshIntervalInMs}).");
    }

    #endregion Public Constructors
  }
}
