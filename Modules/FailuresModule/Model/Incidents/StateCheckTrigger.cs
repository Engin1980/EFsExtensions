using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using ESystem.Asserting;
using EXmlLib.Interfaces;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Incidents
{
  public class CheckStateTrigger : Trigger, IXmlObjectPostDeserialize
  {
    public IStateCheckItem Condition { get; set; }

    public void PostDeserialize()
    {
      EAssert.IsTrue(Condition != null);
    }
  }
}
