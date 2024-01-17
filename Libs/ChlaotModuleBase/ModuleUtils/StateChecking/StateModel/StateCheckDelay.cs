using ESystem.Asserting;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel
{
  public class StateCheckDelay : IStateCheckItem, IXmlObjectPostDeserialize
  {
    public string Seconds { get; set; } = null!;
    public bool IsVariableBased { get => Seconds[0] == '{'; }
    public IStateCheckItem Item { get; set; } = null!;
    public string DisplayString => $"(delay={Seconds} {Item.DisplayString})";
    public double GetSecondsAsDouble() => Double.Parse(Seconds, CultureInfo.GetCultureInfo("en-US"));

    public string GetSecondsAsVariableName()
    {
      EAssert.IsTrue(IsVariableBased);
      return Seconds[1..^1];
    }

    public void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Seconds);
      string numPattern = @"\\d+(\\.\\d+)?";
      string varPattern = @"\\{\\S\\}";
      EAssert.IsTrue(Regex.IsMatch(this.Seconds, numPattern) || Regex.IsMatch(this.Seconds, varPattern));
    }
  }
}
