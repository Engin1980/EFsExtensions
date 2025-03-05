using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eng.EFsExtensions.Modules.CopilotModule.Types
{
  public class Speech
  {
    public enum SpeechType
    {
      Speech,
      File
    }

    private const string VARIABLE_NAME_REGEX = @"\{(.+)\}";
    public byte[] Bytes { get; set; } = null!;
    public SpeechType Type { get; set; }
    public string Value { get; set; } = null!;

    internal string GetEvaluatedValue(VariableVMS variables)
    {
      if (Type != SpeechType.Speech)
        throw new ApplicationException("Not possible to call this function on non-speech-type speech.");
      string ret = this.Value;

      string me(Match m)
      {
        var varName = m.Groups[1].Value;
        var varVal = variables[varName];
        string ret = " " + varVal.ToString() + " ";
        return ret;
      }

      Regex regex = new(VARIABLE_NAME_REGEX);
      ret = regex.Replace(ret, me);

      return ret;
    }

    internal List<string> GetUsedVariables()
    {
      List<string> ret = new();
      if (Type == SpeechType.Speech)
      {
        Regex regex = new Regex(VARIABLE_NAME_REGEX);
        Match m = regex.Match(Value);
        while (m.Success)
        {
          ret.Add(m.Groups[1].Value);
          m = m.NextMatch();
        }
      }
      return ret;
    }
  }
}
