using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ESimConnect
{
  public class ESimConnect : IDisposable
  {
    public class ESimConnectDataReceivedEventArgs
    {
      public object Data { get; set; }

      public int? RequestId { get; set; }

      public ESimConnectDataReceivedEventArgs(int? requestId, object data)
      {
        RequestId = requestId;
        Data = data ?? throw new ArgumentNullException(nameof(data));
      }
    }

    private class RegisteredType
    {
      public uint Id { get; set; }

      public Type Type { get; set; }

      public RegisteredType(uint id, Type type)
      {
        Id = id;
        Type = type ?? throw new ArgumentNullException(nameof(type));
      }
    }

    private enum EEnum
    {
      Unused = 0
    }

    public delegate void ESimConnectDataReceived(ESimConnect sender, ESimConnectDataReceivedEventArgs e);
    public delegate void ESimConnectDelegate(ESimConnect sender);
    public delegate void ESimConnectExceptionDelegate(ESimConnect sender, SIMCONNECT_EXCEPTION ex);

    public event ESimConnectDelegate? Connected;
    public event ESimConnectDataReceived? DataReceived;
    public event ESimConnectDelegate? Disconnected;
    public event ESimConnectExceptionDelegate? ThrowsException;

    /// <summary>
    /// Defines client-defined datum ID. The default is zero
    /// </summary>
    /// <remarks>
    /// For more see https://docs.flightsimulator.com/html/Programming_Tools/SimConnect/API_Reference/Events_And_Data/SimConnect_AddToDataDefinition.htm
    /// </remarks>
    private const int DEFAULT_DATUM_ID_AS_ZERO = 0;

    /// <summary>
    /// Predefined windows handler id to recognize requests from Simconnect. For more see API docs.
    /// </summary>
    private const int WM_USER_SIMCONNECT = 0x0402;
    private readonly List<RegisteredType> registeredTypes = new();
    private readonly HashSet<IntPtr> registeredWindowsQueueHandles = new();
    private readonly Dictionary<int, int> requestIds = new();
    private int nextRequestId = 1;
    private SimConnect? simConnect;
    private IntPtr windowHandle;
    public bool IsOpened { get => this.simConnect != null; }

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

    public void Open()
    {
      if (this.simConnect != null)
        throw new InvalidRequestException("SimConnect already opened.");

      this.windowHandle = GetCurrentWindowHandle();
      try
      {
        RegisterWindowsQueueHandle();
      }
      catch (Exception ex)
      {
        throw new InternalException("Failed to register windows queue handler.", ex);
      }

      try
      {
        this.simConnect = new SimConnect("ESimConnect", windowHandle, WM_USER_SIMCONNECT, null, 0);
        this.simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
        this.simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
        this.simConnect.OnRecvException += SimConnect_OnRecvException;
        this.simConnect.OnRecvSimobjectDataBytype += SimConnect_OnRecvSimobjectDataBytype;
      }
      catch (Exception ex)
      {
        throw new InternalException("Unable to open connection to FS2020.", ex);
      }
    }

    public void UnregisterType<T>()
    {
      if (this.simConnect == null) throw new NotConnectedException();

      Type t = typeof(T);
      RegisteredType? rt = this.registeredTypes.FirstOrDefault(q => q.Type == t);
      if (rt == null)
        throw new InvalidRequestException($"Unable to unregister type '{t.Name}' as it has not been registered yet.");

      uint typeId = rt.Id;
      EEnum eTypeId = (EEnum)typeId;
      this.simConnect.ClearDataDefinition(eTypeId);
    }

    public void RegisterType<T>(uint typeId) where T : struct
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = (EEnum)typeId;
      int epsilon = 0;

      Type t = typeof(T);
      EnsureTypeHasRequiredAttribute(t);
      var fields = t.GetFields();

      foreach (var field in fields)
      {
        DataDefinitionAttribute att = field.GetCustomAttribute<DataDefinitionAttribute>() ??
          throw new InvalidRequestException($"Field '{field.Name}' has not the required '{nameof(DataDefinitionAttribute)}' attribute.");
        EnsureFieldHasCorrectType(field, att);
      }
      foreach (var field in fields)
      {
        DataDefinitionAttribute att = field.GetCustomAttribute<DataDefinitionAttribute>()!;
        try
        {
          simConnect.AddToDataDefinition(eTypeId,
            att.Name, att.Unit, att.Type,
            epsilon, DEFAULT_DATUM_ID_AS_ZERO);
        }
        catch (Exception ex)
        {
          throw new InternalException("Failed to invoke 'simConnect.AddToDataDefinition(...)'.", ex);
        }
      }
      try
      {
        this.simConnect.RegisterDataDefineStruct<T>(eTypeId);
        this.registeredTypes.Add(new RegisteredType(typeId, t));
      }
      catch (Exception ex)
      {
        throw new InternalException("Failed to invoke 'simConnect.RegisterDataDefineStruct<T>(...)'.", ex);
      }
    }

    public void RequestData<T>(
          int? customRequestId = null, uint radius = 0,
      SIMCONNECT_SIMOBJECT_TYPE simObjectType = SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT)
    {
      if (this.simConnect == null) throw new NotConnectedException();

      Type t = typeof(T);
      RegisteredType rt = registeredTypes.FirstOrDefault(q => q.Type == t) ??
        throw new InvalidRequestException($"Type '{t.Name}' has not been registered.");


      int thisRequestId;
      lock (this)
      {
        thisRequestId = nextRequestId++;
      }
      if (customRequestId != null)
      {
        requestIds[thisRequestId] = customRequestId.Value;
      }

      EEnum eRequestId = (EEnum)thisRequestId;
      EEnum eDefineId = (EEnum)rt.Id;
      this.simConnect.RequestDataOnSimObjectType(eRequestId, eDefineId, radius, simObjectType);
    }

    protected IntPtr DefWndProc(IntPtr _hwnd, int msg, IntPtr _wParam, IntPtr _lParam, ref bool handled)
    {
      handled = false;

      if (msg == WM_USER_SIMCONNECT)
      {
        if (simConnect != null)
        {
          simConnect.ReceiveMessage();
          handled = true;
        }
      }
      return (IntPtr)0;
    }

    private static void EnsureFieldHasCorrectType(FieldInfo field, DataDefinitionAttribute att)
    {
      var fieldType = field.FieldType;
      var simType = att.Type;
      if (fieldType == typeof(string) &&
        (simType != SIMCONNECT_DATATYPE.STRING8
        && simType != SIMCONNECT_DATATYPE.STRING32
        && simType != SIMCONNECT_DATATYPE.STRING64
        && simType != SIMCONNECT_DATATYPE.STRING128
        && simType != SIMCONNECT_DATATYPE.STRING256
        && simType != SIMCONNECT_DATATYPE.STRING260))
        throw new InvalidRequestException($"If the field '{field.Name}' is of type string, " +
          $"the expected sim-type should be string too (but declared type is '{simType}'.");
    }

    private static void EnsureTypeHasRequiredAttribute(Type t)
    {
      StructLayoutAttribute? sla = t.StructLayoutAttribute;

      bool hasValidAttribute =
        (sla!.Value == LayoutKind.Sequential)
        && (sla!.CharSet == CharSet.Ansi)
        && (sla!.Pack == 1);

      if (!hasValidAttribute)
      {
        throw new InvalidRequestException($"" +
          $"Struct '{t.Name}' must have [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] attribute defined, " +
          $"bud provided is [StructLayout({sla!.Value}, CharSet = {sla!.CharSet}, Pack = {sla!.Pack})]" +
          $"See: https://docs.flightsimulator.com/html/Programming_Tools/SimConnect/Programming_SimConnect_Clients_using_Managed_Code.htm ");
      }
    }

    private static IntPtr GetCurrentWindowHandle()
    {
      Window window = Application.Current.Windows.OfType<Window>().First(q => q.IsActive);
      IntPtr ret = new WindowInteropHelper(window).Handle;
      return ret;
    }

    private void RegisterWindowsQueueHandle()
    {
      IntPtr handle = GetCurrentWindowHandle();
      if (registeredWindowsQueueHandles.Contains(handle)) return;

      HwndSource lHwndSource = HwndSource.FromHwnd(handle);
      lHwndSource.AddHook(new HwndSourceHook(DefWndProc));

      this.registeredWindowsQueueHandles.Add(handle);
    }

    private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
    {
      SIMCONNECT_EXCEPTION ex = (SIMCONNECT_EXCEPTION)data.dwException;
      ThrowsException?.Invoke(this, ex);
    }

    private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
      this.Connected?.Invoke(this);
    }

    private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
    {
      this.Disconnected?.Invoke(this);
    }

    private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
    {
      int iRequest = (int)data.dwRequestID;
      object ret = data.dwData[0];

      int? userRequestId = requestIds.ContainsKey(iRequest) ? requestIds[iRequest] : null;

      ESimConnectDataReceivedEventArgs e = new(userRequestId, ret);
      this.DataReceived?.Invoke(this, e);
    }
  }
}
