using AffinityModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis.TtsEngine;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class ProcessInfo
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string? WindowTitle { get; set; }
    public int ThreadCount { get; set; }
    public IntPtr? Affinity { get; set; }
    public string? RuleTitle { get; set; }

    public bool? IsAccessible { get; set; }
    public bool[] CoreFlags
    {
      get
      {
        bool[] ret;
        if (this.Affinity == null)
          ret = Array.Empty<bool>();
        else
        {
          ret = AffinityUtils.ToArray(this.Affinity.Value);
        }
        return ret;
      }
    }
    public string StateString
    {
      get
      {
        string ret;

        if (this.RuleTitle == null)
          ret = "No rule to apply";
        else if (this.IsAccessible == null)
          ret = "Not applied";
        else
          ret = this.IsAccessible.Value ? "Applied" : "Access denied.";

        return ret;
      }
    }
  }
}
