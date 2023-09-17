using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class AffinityUtils
  {
    private static readonly int coresCount;

    static AffinityUtils()
    {
      int coreCount = 0;
      foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
      {
        coreCount += int.Parse(item["NumberOfLogicalProcessors"].ToString());
      }
      AffinityUtils.coresCount = coreCount;
    }

    public static bool[] ToArray(int affinity)
    {
      bool[] ret = Convert
            .ToString(affinity, 2)
            .PadLeft(Environment.ProcessorCount, '0')
            .ToCharArray()
            .Reverse()
            .Select(q => q == '1')
            .ToArray();
      return ret;
    }

    public static bool[] ToArray(IntPtr affinity) => ToArray((int)affinity);

    public static IntPtr ToIntPtr(bool[] flags)
    {
      BitArray bitArray = new(flags);
      int intLen = (int)Math.Ceiling(coresCount / 8d);
      int[] tmp = new int[intLen];
      bitArray.CopyTo(tmp, 0);
      IntPtr ret = (IntPtr)tmp[0];
      return ret;
    }

    internal static List<bool> ToEmptyArray(bool defaultValue)
    {
      var ret = new List<bool>();
      for (int i = 0; i < coresCount; i++)
        ret.Add(defaultValue);
      return ret; 
    }
  }
}
