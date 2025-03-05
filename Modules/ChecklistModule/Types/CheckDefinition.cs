using ESystem.Asserting;
using EXmlLib.Attributes;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.ChecklistModule.Types
{
  public class CheckDefinition : IXmlObjectPostDeserialize
  {
    public enum CheckDefinitionType
    {
      Speech,
      File
    }
    public byte[] Bytes { get; set; } = null!;
    public string Value { get; set; } = null!;
    public CheckDefinitionType Type { get; set; }

    public void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Value);
    }
  }
}
