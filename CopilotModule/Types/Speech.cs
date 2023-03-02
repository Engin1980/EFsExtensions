using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CopilotModule.Types
{
  public class Speech
  {
    public enum SpeechType
    {
      Speech,
      File
    }

    public SpeechType Type { get; set; }
    public string Value { get; set; }

    public byte[] Bytes { get; set; }

    internal List<string> GetUsedVariables()
    {
      List<string> ret = new();
      if (Type == SpeechType.Speech)
      {
        string varRegex = @"\{(.+)\}";
        Regex regex = new Regex(varRegex);
        Match m = regex.Match(Value);
        while (m.Success)
        {
          ret.Add(m.Groups[1].Value);
          m = m.NextMatch();
        }
      }
      return ret;
    }

    internal string GetEvaluatedValue(List<Variable> variables)
    {
      string ret;
      ret = here implement; Regex; replace;
      return ret;
    }
  }
}
