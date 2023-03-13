using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.KeyHooking
{
  public class KeyHookInfo
  {
    public const uint MOD_NONE = 0x0000; //[NONE]
    public const uint MOD_ALT = 0x0001; //ALT
    public const uint MOD_CONTROL = 0x0002; //CTRL
    public const uint MOD_SHIFT = 0x0004; //SHIFT
    public const uint MOD_WIN = 0x0008; //WINDOWS

    public KeyHookInfo(KeyModifiers modifiers, Key key, bool markHandled = false)
    {
      Modifiers = modifiers;
      Key = key;
      MarkHandled = markHandled;
    }
    public KeyHookInfo(bool alt, bool ctrl, bool shift, Key key, bool markHandled = false)
    {
      Modifiers = (alt ? KeyModifiers.Alt : KeyModifiers.None)
        | (ctrl ? KeyModifiers.Control : KeyModifiers.None)
        | (shift ? KeyModifiers.Shift : KeyModifiers.None);
      Key = key;
      this.MarkHandled = markHandled;
    }

    public KeyModifiers Modifiers { get; private set; }
    public Key Key { get; private set; }
    public bool MarkHandled { get; set; }
    public uint GetWin32Modifiers()
    {
      uint ret = MOD_NONE;
      if ((Modifiers & KeyModifiers.Alt) > 0)
        ret += MOD_ALT;
      if ((Modifiers & KeyModifiers.Control) > 0)
        ret += MOD_CONTROL;
      if ((Modifiers & KeyModifiers.Shift) > 0)
        ret += MOD_SHIFT;
      if ((Modifiers & KeyModifiers.Win) > 0)
        ret += MOD_WIN;
      return ret;
    }

    internal uint GetVirtualKey() => (uint)KeyInterop.VirtualKeyFromKey(this.Key);
    public override string ToString()
    {
      string ret = Modifiers == KeyModifiers.None
        ? $"{this.Key}"
        : $"{this.Modifiers} + {this.Key}";
      return ret;
    }
  }
}
