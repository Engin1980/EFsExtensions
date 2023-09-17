using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELogging
{
  public delegate void NewLogHandler(LogLevel level, string message);
}
