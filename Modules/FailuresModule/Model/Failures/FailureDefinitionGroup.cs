using ESystem.Asserting;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Failures
{
  public class FailureDefinitionGroup : FailureDefinitionBase, IXmlObjectPostDeserialize
  {
    public string Title { get; set; } = null!;
    public List<FailureDefinitionBase> Items { get; set; } = new List<FailureDefinitionBase>();

    public void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Title);
      EAssert.IsTrue(Items.Any());
    }
  }
}
