using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking
{
  public enum StateCheckConditionOperator
  {
    And,
    Or
  }

  public enum StateCheckPropertyDirection
  {
    Above,
    Below,
    Exactly,
    Passing,
    PassingDown,
    PassingUp
  }

}
