using ESystem.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.KeyHooking
{
  public class KeyHookWrapper : IDisposable
  {
    public delegate void KeyHookInvokedDelegate(int hookId, KeyHookInfo keyHookInfo);

    public event KeyHookInvokedDelegate? KeyHookInvoked;

    private const int KEY_HOOK_ID_BASE = 132654;

    private static int nextKeyHookIdShift = 0;

    private readonly Dictionary<int, KeyHookInfo> registeredHooks = new();

    private readonly HwndSource source;

    private readonly Window window;

    private readonly IntPtr windowHandle;
    private readonly Logger logger;

    public KeyHookWrapper()
    {
      this.logger = Logger.Create(this);
      this.window = Application.Current.Windows.OfType<Window>().First(x => x.IsActive);
      this.windowHandle = new WindowInteropHelper(window).Handle;
      this.source = HwndSource.FromHwnd(this.windowHandle);
      this.source.AddHook(HwndHook);
    }

    public void Dispose()
    {
      this.source.RemoveHook(HwndHook);
      UnregisterAllKeyHooks();
      Logger.UnregisterSender(this);
      GC.SuppressFinalize(this);
    }

    public int RegisterKeyHook(KeyHookInfo keyHookInfo)
    {
      int id = KEY_HOOK_ID_BASE + nextKeyHookIdShift++;
      this.logger.Log(LogLevel.INFO, $"Registering keyhook {keyHookInfo} as id={id}.");
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

    public void UnregisterAllKeyHooks()
    {
      this.registeredHooks.Keys.ToList().ForEach(q => UnregisterKeyHook(q));
    }

    public void UnregisterKeyHook(int id)
    {
      this.logger.Invoke(LogLevel.INFO, $"UNRegistering keyhook id={id}.");
      UnregisterHotKey(this.windowHandle, id);
      this.registeredHooks.Remove(id);
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

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
  }
}