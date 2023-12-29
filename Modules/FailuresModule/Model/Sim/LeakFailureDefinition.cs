using ESystem.Asserting;
using System.Windows.Media.Animation;

namespace FailuresModule.Model.Sim
{
  public class LeakFailureDefinition : FailureDefinition
  {
    #region Private Fields

    private const int DEFAULT_MAXIMUM_LEAK_TICKS = 100 * 60;
    private const int DEFAULT_MINIMUM_LEAK_TICKS = 5 * 60;
    private const int DEFAULT_TICK_LENGTH_IN_MS = 1000;

    #endregion Private Fields

    #region Public Properties
    public int MaximumLeakTicks { get; set; } = DEFAULT_MAXIMUM_LEAK_TICKS;
    public int MinimumLeakTicks { get; set; } = DEFAULT_MINIMUM_LEAK_TICKS;
    public int TickLengthInMs { get; set; } = DEFAULT_TICK_LENGTH_IN_MS;
    public override string Type => "Leak";

    #endregion Public Properties

    #region Public Constructors

    public LeakFailureDefinition(string id, string title, string simConPoint) : base(id, title, simConPoint)
    {
    }

    public void EnsureValid()
    {
      EAssert.IsTrue(MaximumLeakTicks > MinimumLeakTicks);
      EAssert.IsTrue(MinimumLeakTicks > 0);
      EAssert.IsTrue(TickLengthInMs > 50);
    }

    #endregion Public Constructors
  }
}
