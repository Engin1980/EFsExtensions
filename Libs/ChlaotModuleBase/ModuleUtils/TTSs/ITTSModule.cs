using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs
{
  public interface ITtsModule
  {
    public ITts Tts { get; }
    public string Name { get; }
    public System.Windows.Controls.UserControl SettingsControl { get; }
  }
}
