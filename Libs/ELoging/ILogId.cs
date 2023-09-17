using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELogging
{
  public class LogIdAble
  {
    private static int nextLogId;
    public int LogId { get; private set; } = nextLogId++;
  }
}
