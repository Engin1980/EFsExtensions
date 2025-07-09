using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.StateModel
{
  public class StateCheckCondition : IStateCheckItem
  {
    public List<IStateCheckItem> Items { get; set; } = null!;
    public StateCheckConditionOperator Operator { get; set; }
    public string DisplayString => $"({Operator} {string.Join(", ", Items.Select(q => q.DisplayString))})";

    public override string ToString() => this.DisplayString + " {StateCheckCondition}";
  }
}
