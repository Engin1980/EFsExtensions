using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class Variable
  {
    [EXmlNonemptyString]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public abstract double Value { get; }
  }
}
