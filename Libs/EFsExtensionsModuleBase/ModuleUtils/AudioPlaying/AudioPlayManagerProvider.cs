using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying
{
  public class AudioPlayManagerProvider
  {
    private readonly static AudioPlayManager instance = new AudioPlayManager();
    public static AudioPlayManager Instance => instance;
  }
}
