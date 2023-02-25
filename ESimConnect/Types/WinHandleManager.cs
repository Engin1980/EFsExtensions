using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ESimConnect.Types
{
  public class WinHandleManager : IDisposable
  {
    /// <summary>
    /// Predefined windows handler id to recognize requests from Simconnect. For more see API docs.
    /// </summary>
    public const int WM_USER_SIMCONNECT = 0x0402;

    private Window? window = null;
    private IntPtr windowHandle = IntPtr.Zero;
    private SimConnect? _SimConnect = null;
    public SimConnect? SimConnect { get => _SimConnect; set => _SimConnect = value; }

    public delegate void FsExitDetectedDelegate();
    public event FsExitDetectedDelegate? FsExitDetected;

    public IntPtr Handle
    {
      get
      {
        if (window == null)
          throw new InvalidRequestException("Cannot get win-handle when window is not created.");
        return windowHandle;
      }
    }

    public void Acquire()
    {
      CreateWindow();
      HwndSource lHwndSource = HwndSource.FromHwnd(this.windowHandle);
      lHwndSource.AddHook(new HwndSourceHook(DefWndProc));
    }


    protected IntPtr DefWndProc(IntPtr _hwnd, int msg, IntPtr _wParam, IntPtr _lParam, ref bool handled)
    {
      handled = false;

      if (msg == WM_USER_SIMCONNECT)
      {
        if (this._SimConnect != null)
        {
          try
          {
            this._SimConnect.ReceiveMessage();
          }
          catch (Exception ex)
          {
            if (ex is System.Runtime.InteropServices.COMException && ex.Message == "0xC00000B0")
            {
              FsExitDetected?.Invoke();
            }
            else
            {
              throw new InternalException("Failed to invoke SimConnect.ReceiveMessage().", ex);
            }
          }
          handled = true;
        }
      }
      return (IntPtr)0;
    }

    public void Release()
    {
      if (this.window != null)
      {
        this.window.Close();
        this.window = null;
      }
      this.windowHandle = IntPtr.Zero;
    }

    private void CreateWindow()
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        this.window = new Window();
        var wih = new WindowInteropHelper(window);
        wih.EnsureHandle();
        this.windowHandle = new WindowInteropHelper(this.window).Handle;
      });
      while (this.window == null)
        System.Threading.Thread.Sleep(50);
    }

    public void Dispose()
    {
      Release();
    }
  }
}
