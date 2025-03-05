using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using ESystem.Asserting;
using EXmlLib.Interfaces;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents
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
