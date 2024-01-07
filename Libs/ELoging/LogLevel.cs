using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELogging
{
    [Flags]
    public enum LogLevel
    {
        Unused = 0,
        VERBOSE = 1,
        INFO = 2,
        WARNING = 4,
        ERROR = 8
    }
}
