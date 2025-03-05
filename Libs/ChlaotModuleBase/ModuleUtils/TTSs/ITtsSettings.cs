using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs
{
  public interface ITtsSettings : INotifyPropertyChanged
  {
    public bool IsValid { get; }

    public void LoadFromSettingsString(string str);
    public string CreateSettingsString();
  }
}
