using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFsExtensionsModuleBase.ModuleUtils.StateChecking
{
  [AttributeUsage(AttributeTargets.Property)]
  public class StateCheckNameAttribute : Attribute
  {
    public string Name { get; set; }

    public StateCheckNameAttribute(string name)
    {
      Name = name;
    }
  }
}
