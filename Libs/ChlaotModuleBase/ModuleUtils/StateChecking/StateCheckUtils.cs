using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public static class StateCheckUtils
  {
    public record VariableUsage(string VariableName, StateCheckProperty Property);    

    public static List<VariableUsage> ExtractVariables(params IStateCheckItem[] stateCheckItem)
    {
      List<VariableUsage> ret = new();
      Stack<IStateCheckItem> stack = new();
      stateCheckItem.ToList().ForEach(q => stack.Push(q));
      while (stack.Count > 0)
      {
        IStateCheckItem sci = stack.Pop();
        if (sci is StateCheckCondition scic)
          scic.Items.ForEach(q => stack.Push(q));
        else if (sci is StateCheckDelay scid)
          stack.Push(scid.Item);
        else if ((sci is StateCheckProperty scip) && scip.IsVariableBased)
          ret.Add(new(scip.GetExpressionAsVariableName(), scip));
      }
      return ret;
    }
  }
}
