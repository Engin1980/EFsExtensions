using ESystem.Asserting;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.ChecklistModule.Types
{
  public class CheckItem : IXmlObjectPostDeserialize
  {
    public CheckDefinition Call { get; set; } = null!;
    public CheckDefinition Confirmation { get; set; } = null!;

    public void PostDeserialize()
    {
      EAssert.IsNotNull(Call);
      EAssert.IsNotNull(Confirmation);
    }
  }
}
