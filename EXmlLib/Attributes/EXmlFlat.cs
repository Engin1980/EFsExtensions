using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXmlLib.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class EXmlFlat : Attribute
  {
    public EXmlFlat(string? elementName)
    {
      ElementName = elementName;
    }

    public string? ElementName { get; set; } = null;
  }
}
