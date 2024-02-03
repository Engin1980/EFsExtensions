using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel
{
  public class StateCheckWait : IStateCheckItem
  {
    public string Seconds { get; set; } = null!;
    public bool IsVariableBased { get => Seconds[0] == '{'; }
    public IStateCheckItem Item { get; set; } = null!;
    public string DisplayString => $"(delay={Seconds} {Item.DisplayString})";
    public double GetSecondsAsDouble() => double.Parse(Seconds, CultureInfo.GetCultureInfo("en-US"));

    public string GetSecondsAsVariableName()
    {
      EAssert.IsTrue(IsVariableBased);
      return Seconds[1..^1];
    }

    public void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Seconds);
      string numPattern = @"^\d+(\.\d+)?$";
      string varPattern = @"^\{\S+\}$";
      EAssert.IsTrue(
        Regex.IsMatch(Seconds, numPattern) || Regex.IsMatch(Seconds, varPattern),
        $"Value of seconds ('{Seconds}') is not in the valid format.");
    }

  }
}
