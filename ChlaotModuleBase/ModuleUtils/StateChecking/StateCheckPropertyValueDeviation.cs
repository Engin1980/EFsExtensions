using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckPropertyValueDeviation
  {
    private string _Definition = string.Empty;

    public StateCheckPropertyValueDeviation(string definition)
    {
      this.Definition = definition;
    }

    public string Definition
    {
      get => _Definition; set
      {
        _Definition = value;
        (string prefix, string val, string postfix) = Decode(value);

        IsPercentage = postfix == "%";
        IsAbove = prefix.Contains('+');
        IsBelow = prefix.Contains('-');
        Value = double.Parse(val);
      }
    }

    private (string prefix, string val, string postfix) Decode(string value)
    {
      int firstDigitIndex = -1;
      int lastDigitIndex = -1;
      for (int i = 0; i < value.Length; i++)
      {
        char c = value[i];
        if (char.IsDigit(c) || c == '.')
        {
          if (firstDigitIndex == -1)
            firstDigitIndex = i;
          lastDigitIndex = i;
        }
      }

      string prefix = value.Substring(0, firstDigitIndex);
      string postfix = value.Substring(lastDigitIndex + 1);
      string val = value.Substring(firstDigitIndex, lastDigitIndex - firstDigitIndex);
      return (prefix, val, postfix);
    }

    public bool IsPercentage { get; private set; } = false;
    public bool IsAbove { get; private set; } = false;
    public bool IsBelow { get; private set; } = false;
    public double Value { get; private set; } = 0;
  }
}
