using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Exceptions;
using ESystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel
{
  public class StateCheckPropertyDeviation
  {
    public record StateCheckPropertyDeviationValue(double Value, bool IsPercentage)
    {
      internal double GetValue(double value)
      {
        double ret = IsPercentage
          ? value * ((100 + this.Value) / 100)
          : value;
        return ret;
      }
    }

    public string Definition
    {
      get
      {
        StringBuilder sb = new();
        if (Above.Value == 0)
          sb.Append('-').Append(Below.Value).AppendIf(Below.IsPercentage, "%");
        else if (Below.Value == 0)
          sb.Append('+').Append(Above.Value).AppendIf(Above.IsPercentage, "%");
        else if (Above.Value == Below.Value && Above.IsPercentage == Below.IsPercentage)
          sb.Append("+-").Append(Above.Value).AppendIf(Above.IsPercentage, "%");
        else
          sb.Append('-').Append(Below.Value).AppendIf(Below.IsPercentage, "%").Append('+').Append(Above.Value).AppendIf(Above.IsPercentage, "%");
        return sb.ToString();
      }
    }

    public StateCheckPropertyDeviationValue Above { get; private set; }
    public StateCheckPropertyDeviationValue Below { get; private set; }

    public StateCheckPropertyDeviation(StateCheckPropertyDeviationValue above, StateCheckPropertyDeviationValue below)
    {
      Above = above ?? throw new ArgumentNullException(nameof(above));
      Below = below ?? throw new ArgumentNullException(nameof(below));
    }

    public static StateCheckPropertyDeviation Parse(string text)
    {
      StateCheckPropertyDeviationValue above;
      StateCheckPropertyDeviationValue below;

      string patternSimple = @"^(\+|\-|\+\-|\-\+)(\d+(\.\d+)?)(\%|p)?$";
      Match matchSimple = Regex.Match(text, patternSimple);

      string patternFull = @"^([\-\+]\d+(\.\d+)?(p|\%)?)([\+\-]\d+(\.\d+)?(p|\%)?)$";
      Match matchFull = Regex.Match(text, patternFull);

      if ((matchSimple.Success))
      {
        double value = double.Parse(matchSimple.Groups[2].Value, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        bool isPercentage = matchSimple.Groups[4].Success;
        above = matchSimple.Groups[1].Value.Contains('+') ? new(value, isPercentage) : new(0, false);
        below = matchSimple.Groups[1].Value.Contains('-') ? new(value, isPercentage) : new(0, false);
      }
      else if (matchFull.Success && matchFull.Groups[2].Value != matchFull.Groups[7].Value)
      {
        double valueA, valueB;
        bool isPercentageA, isPercentageB;

        valueA = double.Parse(matchSimple.Groups[3].Value, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        isPercentageA = matchSimple.Groups[5].Success;
        valueB = double.Parse(matchSimple.Groups[8].Value, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        isPercentageB = matchSimple.Groups[10].Success;

        if (matchFull.Groups[2].Value == "+")
        {
          above = new(valueA, isPercentageA);
          below = new(valueB, isPercentageB);
        }
        else
        {
          below = new(valueA, isPercentageA);
          above = new(valueB, isPercentageB);
        }
      }
      else
        throw new StateCheckDeserializationException($"Failed to parse '{text}' as value-deviation (expected something like +-10% or -200+300p.");

      StateCheckPropertyDeviation ret = new(above, below);
      return ret;
    }

    public override string ToString() => this.Definition;

    internal void CheckIsValid()
    {
      if (Above == null) throw new StateCheckException($"{nameof(Above)} is null.");
      if (Below == null) throw new StateCheckException($"{nameof(Below)} is null.");

    }
  }
}
