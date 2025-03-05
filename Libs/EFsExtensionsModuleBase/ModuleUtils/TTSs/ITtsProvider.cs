using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs
{
  public interface ITtsProvider
  {
    Task<byte[]> ConvertAsync(string text);
    byte[] Convert(string text);
  }
}
