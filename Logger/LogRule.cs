using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELogging
{
  public class LogRule
  {
    public LogLevel AcceptedLogLevels { get; private set; }

    public string Pattern { get; private set; }

    public LogRule(string pattern, LogLevel acceptedLogLevels)
    {
      Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
      AcceptedLogLevels = acceptedLogLevels;
    }

    public LogRule(string pattern, bool logVerbose, bool logInfo, bool logWarning, bool logError)
      : this(pattern, ConvertBoolsToFlag(logVerbose, logInfo, logWarning, logError)) { }

    internal bool IsLogLevelMatch(LogLevel level)
    {
      int tmp = (int)level & (int)AcceptedLogLevels;
      bool ret = tmp > 0;
      return ret;
    }

    internal bool IsPatternMatch(string senderName)
      => System.Text.RegularExpressions.Regex.IsMatch(senderName, this.Pattern);

    private static LogLevel ConvertBoolsToFlag(bool logVerbose, bool logInfo, bool logWarning, bool logError)
    {
      LogLevel ret = LogLevel.Unused;
      if (logVerbose) ret |= LogLevel.VERBOSE;
      if (logInfo) ret |= LogLevel.INFO;
      if (logWarning) ret |= LogLevel.WARNING;
      if (logError) ret |= LogLevel.ERROR;
      return ret;
    }
  }
}
