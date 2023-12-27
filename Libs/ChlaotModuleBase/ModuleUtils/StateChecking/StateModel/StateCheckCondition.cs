using System.Collections.Generic;
using System.Linq;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel
{
  public class StateCheckCondition : IStateCheckItem
  {
    public List<IStateCheckItem> Items { get; set; } = null!;
    public StateCheckConditionOperator Operator { get; set; }
    public string DisplayString => $"({Operator} {string.Join(", ", Items.Select(q => q.DisplayString))})";
  }
}
