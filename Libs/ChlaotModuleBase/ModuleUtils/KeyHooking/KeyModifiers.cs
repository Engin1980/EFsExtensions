using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.KeyHooking
{
  [Flags]
  public enum KeyModifiers
  {
    None = (int)KeyHookInfo.MOD_NONE,
    Alt = (int)KeyHookInfo.MOD_ALT,
    Control = (int)KeyHookInfo.MOD_CONTROL,
    Shift = (int)KeyHookInfo.MOD_SHIFT,
    Win = (int)KeyHookInfo.MOD_WIN
  }
}
