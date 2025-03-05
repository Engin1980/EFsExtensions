using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs
{
  public interface ITtsModule
  {
    public string Name { get; }
    public ITtsProvider GetProvider(ITtsSettings settings);
    public System.Windows.Controls.UserControl GetSettingsControl(ITtsSettings settings);
    public ITtsSettings GetDefaultSettings();
  }
}
