using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using ESystem.Asserting;

namespace FailuresModule.Model.App
{
  public class CheckStateTrigger : Trigger
  {
    public IStateCheckItem Condition { get; set; }

    public void EnsureValid()
    {
      EAssert.IsTrue(Condition != null);
    }
  }
}
