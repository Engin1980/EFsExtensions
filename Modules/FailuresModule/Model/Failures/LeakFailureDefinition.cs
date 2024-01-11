using ESystem.Asserting;
using EXmlLib.Interfaces;
using System.Windows.Media.Animation;

namespace FailuresModule.Model.Failures
{
  public class LeakFailureDefinition : WithSimVarFailureDefinition
  {
    #region Private Fields

    private const int DEFAULT_MAXIMUM_LEAK_TICKS = 100 * 60;
    private const int DEFAULT_MINIMUM_LEAK_TICKS = 5 * 60;
    private const int DEFAULT_TICK_INTERVAL_IN_MS = 1000;

    #endregion Private Fields

    #region Public Properties
    public int MaximumLeakTicks { get; set; } = DEFAULT_MAXIMUM_LEAK_TICKS;
    public int MinimumLeakTicks { get; set; } = DEFAULT_MINIMUM_LEAK_TICKS;
    public int TickIntervalInMs { get; set; } = DEFAULT_TICK_INTERVAL_IN_MS;
    public override string SimConPoint => SimVar;
    public override string Type => "Leak";

    #endregion Public Properties

    #region Methods

    public override void PostDeserialize()
    {
      base.PostDeserialize();
      EAssert.IsTrue(MaximumLeakTicks > MinimumLeakTicks);
      EAssert.IsTrue(MinimumLeakTicks > 0);
      EAssert.IsTrue(TickIntervalInMs > 50);
      EAssert.IsNonEmptyString(SimVar);
    }

    #endregion Methods
  }
}
