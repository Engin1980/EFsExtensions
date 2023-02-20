using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

namespace ESimConnect
{
  public class ESimConnect : IDisposable
  {
    private IntPtr windowHandle;
    private bool winHandleSet = false;
    private SimConnect? simConnect;
    private const int WM_USER_SIMCONNECT = 0x0402;
    private static IntPtr GetCurrentWindowHandle()
    {
      Window window = Application.Current.Windows.OfType<Window>().First(q => q.IsActive);
      IntPtr ret = new WindowInteropHelper(window).Handle;
      return ret;
    }

    public void SetWinHandle()
    {
      IntPtr handle = GetCurrentWindowHandle();
      HwndSource lHwndSource = HwndSource.FromHwnd(handle);
      lHwndSource.AddHook(new HwndSourceHook(DefWndProc));
      winHandleSet = true;
    }

    public void Open()
    {
      this.windowHandle = GetCurrentWindowHandle();
      try
      {
        this.simConnect = new SimConnect("ESimConnect", windowHandle, WM_USER_SIMCONNECT, null, 0);
        this.simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
        this.simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);
        this.simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Unable to open connection to FS2020.", ex);
      }
      if (!winHandleSet)
        SetWinHandle();
    }

    public void Close()
    {
      if (this.simConnect != null)
      {
        this.simConnect.Dispose();
        this.simConnect = null;
      }
    }

    public void Dispose()
    {
      Close();
    }

    protected IntPtr DefWndProc(IntPtr _hwnd, int msg, IntPtr _wParam, IntPtr _lParam, ref bool handled)
    {
      handled = false;

      if (msg == WM_USER_SIMCONNECT)
      {
        if (simConnect != null)
        {
          simConnect.ReceiveMessage();
        }
      }
      return (IntPtr)0;
    }

    public delegate void ESimConnectDelegate(ESimConnect sender);
    public event ESimConnectDelegate? Connected;
    public event ESimConnectDelegate? Disconnected;
    public delegate void ESimConnectExceptionDelegate(ESimConnect sender, SIMCONNECT_EXCEPTION ex);
    public event ESimConnectExceptionDelegate? ThrowsException;

    public enum TypeDef
    {
      Unset = 0
    }

    public void RegisterType<T>(uint id) where T : struct
    {
      if (this.simConnect == null) throw new ApplicationException("SimConnect not connected.");

      Type t = typeof(T);
      var fields = t.GetFields();
      TypeDef td = (TypeDef)4;
      foreach (var field in fields)
      {
        SimConnectDataDefinitionAttribute att = field.GetCustomAttribute<SimConnectDataDefinitionAttribute>();
        if (att != null)
        {
          throw new ApplicationException($"Field '{field.Name}' has not the required SimConnectDataDefinitionAttribute attribute.");
        }

        simConnect.AddToDataDefinition(
          td,
          att.SimPropertyName,
          att.Unit,
          att.Type,
          0,
          SimConnect.SIMCONNECT_UNUSED);
      }
      this.simConnect.RegisterDataDefineStruct<T>(td);
    }

    private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
      this.Connected?.Invoke(this);
    }

    private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
    {
      this.Disconnected?.Invoke(this);
    }

    private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
    {
      SIMCONNECT_EXCEPTION ex = (SIMCONNECT_EXCEPTION)data.dwException;
      ThrowsException?.Invoke(this, ex);
    }
  }
}
