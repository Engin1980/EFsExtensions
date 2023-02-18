using ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ChecklistModule.Support
{
  public class KeyHookWrapper : IDisposable
  {

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public delegate void KeyHookInvokedDelegate(int hookId, KeyHookInfo keyHookInfo);
    public event KeyHookInvokedDelegate? KeyHookInvoked;

    //Modifiers:
    private const uint MOD_NONE = 0x0000; //[NONE]
    private const uint MOD_ALT = 0x0001; //ALT
    private const uint MOD_CONTROL = 0x0002; //CTRL
    private const uint MOD_SHIFT = 0x0004; //SHIFT
    private const uint MOD_WIN = 0x0008; //WINDOWS

    private readonly HwndSource source;
    private readonly Window window;
    private readonly IntPtr windowHandle;
    private readonly Dictionary<int, KeyHookInfo> registeredHooks = new();
    private static int nextKeyHookIdShift = 0;
    private const int KEY_HOOK_ID_BASE = 132654;

    public class KeyHookInfo
    {
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

    [Flags]
    public enum KeyModifiers
    {
      None = 0,
      Alt = 0x0001,
      Control = 0x0002,
      Shift = 0x0004,
      Win = 0x0008
    }

    public KeyHookWrapper(Window relatedWindow)
    {
      this.window = Application.Current.Windows.OfType<Window>().First(x => x.IsActive);
      this.windowHandle = new WindowInteropHelper(window).Handle;
      this.source = HwndSource.FromHwnd(this.windowHandle);
      this.source.AddHook(HwndHook);
    }

    public int RegisterKeyHook(KeyHookInfo keyHookInfo)
    {
      int id = KEY_HOOK_ID_BASE + nextKeyHookIdShift++;
      uint modifiers = keyHookInfo.GetWin32Modifiers();
      uint vkey = keyHookInfo.GetVirtualKey();
      bool res = RegisterHotKey(windowHandle, id, modifiers, vkey);

      if (res == false)
      {
        throw new ApplicationException("Failed to register hotkey " + keyHookInfo);
      }
      this.registeredHooks[id] = keyHookInfo;
      return id;
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      const int WM_HOTKEY = 0x0312;
      if (msg == WM_HOTKEY)
      {
        int id = wParam.ToInt32();
        if (registeredHooks.ContainsKey(id))
        {
          var khi = registeredHooks[id];
          this.KeyHookInvoked?.Invoke(id, khi);
          handled = true;
        }
      }
      return IntPtr.Zero;
    }

    public void UnregisterKeyHook(int id)
    {
      UnregisterHotKey(this.windowHandle, id);
      this.registeredHooks.Remove(id);
    }

    public void UnregisterAllKeyHooks()
    {
      this.registeredHooks.Keys.ToList().ForEach(q => UnregisterKeyHook(q));
    }

    public void Dispose()
    {
      this.source.RemoveHook(HwndHook);
      UnregisterAllKeyHooks();
      GC.SuppressFinalize(this);
    }
  }
}
