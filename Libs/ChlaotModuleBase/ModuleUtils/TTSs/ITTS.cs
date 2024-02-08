using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs
{
  public interface ITts
  {
    bool IsReady { get; }

    Task<byte[]> ConvertAsync(string text);
  }
}
