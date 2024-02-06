using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs
{
  public class TtsApplicationException : ApplicationException
  {
    public TtsApplicationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
  }
}
