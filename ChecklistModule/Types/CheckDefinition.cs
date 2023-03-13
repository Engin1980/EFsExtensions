using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.ChecklistModule.Types
{
  public class CheckDefinition
  {
    public enum CheckDefinitionType
    {
      Speech,
      File
    }
#pragma warning disable CS8618
    public byte[] Bytes { get; set; }
    public string Value { get; set; }
    public CheckDefinitionType Type { get; set; }
#pragma warning restore CS8618
  }
}
