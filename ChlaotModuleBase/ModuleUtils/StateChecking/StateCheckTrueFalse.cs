using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckTrueFalse : IStateCheckItem
  {
    public StateCheckTrueFalse(bool value)
    {
      Value = value;
    }

    public bool Value { get; set; }

    public string DisplayString => Value ? "True" : "False";
  }
}
