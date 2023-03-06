using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class ProcessInfo
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string WindowTitle { get; set; }
    public int ThreadCount { get; set; }
    public int? Affinity { get; set; }
    public string? RuleTitle { get; set; }

    public string AffinityBinaryString
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        if (Affinity != null)
          sb.Append(Convert.ToString((int)Affinity, 2).PadLeft(Environment.ProcessorCount, '0'));

        char[] chars = sb.ToString().ToCharArray();
        Array.Reverse(chars);
        string tmp = new string(chars);

        sb = new StringBuilder();
        for (int i = 0; i < tmp.Length; i++)
        {
          if (i > 0 && i % 4 == 0)
            sb.Append(" ");
          sb.Append(tmp[i]);
        }
        return sb.ToString();
      }
    }
    public bool? IsAccessible { get; set; }
  }
}
