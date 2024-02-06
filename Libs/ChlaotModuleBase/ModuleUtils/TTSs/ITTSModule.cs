using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs
{
  public interface ITTSModule
  {
    public ITTs TTS { get; }
    public System.Windows.Controls.UserControl SettingsControl { get; }
  }
}
