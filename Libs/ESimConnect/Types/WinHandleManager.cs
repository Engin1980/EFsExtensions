using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ESimConnect.Types
{
  public class WinHandleManager : IDisposable
  {
    public delegate void ExceptionRaisedDelegate(Exception ex);
    public event ExceptionRaisedDelegate? ExceptionRaised;

    /// <summary>
    /// Predefined windows handler id to recognize requests from Simconnect. For more see API docs.
    /// </summary>
    public const int WM_USER_SIMCONNECT = 0x0402;

    private Window? window = null;
    private IntPtr windowHandle = IntPtr.Zero;
    private HwndSource? hwndSource = null;
    private HwndSourceHook? hook = null;
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
      Logger.LogMethodStart();
      CreateWindow();
      this.hwndSource = HwndSource.FromHwnd(this.windowHandle);
      this.hook = new HwndSourceHook(DefWndProc);
      this.hwndSource.AddHook(this.hook);
      Logger.LogMethodEnd();
    }


    protected IntPtr DefWndProc(IntPtr _hwnd, int msg, IntPtr _wParam, IntPtr _lParam, ref bool handled)
    {
      handled = false;

      if (msg == WM_USER_SIMCONNECT)
      {
        if (this._SimConnect != null && this.hwndSource != null)
        {
          Logger.Log("DefWndProc", this);
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
              string s = ExpandExceptionString(ex);
              System.IO.File.WriteAllText("error.txt", s); //TODO remove when not used
              ELogging.Logger.Log(this, ELogging.LogLevel.ERROR, "DefWndProc EXCEPTION " + s);
              this.ExceptionRaised?.Invoke(ex);
            }
          }
          handled = true;
        }
      }
      return (IntPtr)0;
    }

    private void TryInvokExceptionInMainThread(Exception newException)
    {
      //TODO remove if not used
      Application.Current.Dispatcher.Invoke(() => throw newException);
    }

    private string ExpandExceptionString(Exception ex)
    {
      List<string> tmp = new();
      while (ex != null)
      {
        tmp.Add(ex.Message);
        tmp.Add("\n\t");
        tmp.Add(ex.StackTrace ?? "");
        ex = ex.InnerException!;
      }
      string ret = string.Join("\n\n", tmp);
      return ret;
    }

    public void Release()
    {
      void destroyWindowHandle()
      {
        if (this.hwndSource != null)
        {
          this.hwndSource.RemoveHook(this.hook);
          this.hook = null;
          this.hwndSource = null;
        }

        if (this.window != null)
        {
          this.window.Close();
          this.window = null;
        }
        this.windowHandle = IntPtr.Zero;
      };
      if (Application.Current == null)
        destroyWindowHandle();
      else
        Application.Current.Dispatcher.Invoke(() => destroyWindowHandle());

      while (this.windowHandle != IntPtr.Zero)
        System.Threading.Thread.Sleep(50);
    }

    private void CreateWindow()
    {
      void createWindowHandle()
      {
        this.windowHandle = Process.GetCurrentProcess().MainWindowHandle;
        this.window = new Window();
        var wih = new WindowInteropHelper(window);
        wih.EnsureHandle();
        this.windowHandle = new WindowInteropHelper(this.window).Handle;
      };

      if (Application.Current == null)
        createWindowHandle();
      else
        Application.Current.Dispatcher.Invoke(() => createWindowHandle());

      while (this.window == null)
        System.Threading.Thread.Sleep(50);
    }

    public void Dispose()
    {
      Release();
    }
  }
}
