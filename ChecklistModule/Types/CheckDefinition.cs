using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Types
{
  public class CheckDefinition
  {
    public enum CheckDefinitionType
    {
      Speech,
      File
    }

    public byte[] Bytes { get; set; }
    public string Value { get; set; }
    public CheckDefinitionType Type { get; set; }
  }
}
