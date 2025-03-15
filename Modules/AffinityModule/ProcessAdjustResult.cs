using Eng.EFsExtensions.Modules.AffinityModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis.TtsEngine;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.AffinityModule
{
  public class ProcessAdjustResult
  {
    public enum EResult
    {
      Unchanged,
      Ok,
      Failed
    }

    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? WindowTitle { get; set; }
    public int ThreadCount { get; set; }
    public IntPtr? AffinityPre { get; set; }
    public IntPtr? AffinityPost { get; set; }
    public AffinityRule? AffinityRule { get; set; }
    public PriorityRule? PriorityRule { get; set; }

    public EResult AffinitySetResult { get; set; }
    public EResult AffinityGetResult { get; set; }

    public ProcessPriorityClass? PriorityPre { get; set; }
    public ProcessPriorityClass? PriorityPost { get; set; }
    public EResult PrioritySetResult { get; set; }
    public EResult PriorityGetResult { get; set; }

    public bool[] CoreFlagsPre
    {
      get
      {
        bool[] ret;
        if (this.AffinityPre == null)
          ret = Array.Empty<bool>();
        else
        {
          ret = AffinityUtils.ToArray(this.AffinityPre.Value);
        }
        return ret;
      }
    }
    public bool[] CoreFlagsPost
    {
      get
      {
        bool[] ret;
        if (this.AffinityPost == null)
          ret = Array.Empty<bool>();
        else
        {
          ret = AffinityUtils.ToArray(this.AffinityPost.Value);
        }
        return ret;
      }
    }
    public string StateString
    {
      get
      {
        string ret;

        ret = "TODO ProcessInfo line 55";

        //if (this.RuleTitle == null)
        //  ret = "No rule to apply";
        //else if (this.IsAccessible == null)
        //  ret = "Not applied";
        //else
        //  ret = this.IsAccessible.Value ? "Applied" : "Access denied.";

        return ret;
      }
    }
  }
}
