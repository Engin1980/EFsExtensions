using System.Collections.Generic;
using System.Linq;

namespace ChecklistModule.Types.Autostarts
{
  public class AutostartCondition : IAutostart
  {
    public List<IAutostart> Items { get; set; }
    public AutostartConditionOperator Operator { get; set; }

    public string DisplayString => $"({Operator} {string.Join(", ", Items.Select(q=>q.DisplayString))})";
  }
}
