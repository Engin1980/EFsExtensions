using Microsoft.FlightSimulator.SimConnect;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
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

    private readonly static Dictionary<Type, SIMCONNECT_DATATYPE> typeMapping;

    private readonly static Dictionary<int, SIMCONNECT_DATATYPE> typeStringMapping;

    private readonly List<RegisteredType> registeredTypes = new();

    private readonly HashSet<IntPtr> registeredWindowsQueueHandles = new();

    private readonly Dictionary<int, int> requestIds = new();

    private int nextRequestId = 1;

    private SimConnect? simConnect;

    private Window window;

    private IntPtr windowHandle;

    public bool IsOpened { get => this.simConnect != null; }

    static ESimConnect()
    {
      typeMapping = new(){
        {typeof(int) , SIMCONNECT_DATATYPE.INT32},
        {typeof(long) , SIMCONNECT_DATATYPE.INT64},
        {typeof(float) , SIMCONNECT_DATATYPE.FLOAT32},
        {typeof(double) , SIMCONNECT_DATATYPE.FLOAT64 }
      };
      typeStringMapping = new Dictionary<int, SIMCONNECT_DATATYPE>()
      {
        {8, SIMCONNECT_DATATYPE.STRING8},
        {32, SIMCONNECT_DATATYPE.STRING32 },
        {64, SIMCONNECT_DATATYPE.STRING64 },
        {128, SIMCONNECT_DATATYPE.STRING128 },
        {256, SIMCONNECT_DATATYPE.STRING256 },
        {260, SIMCONNECT_DATATYPE.STRING260 } };
    }

    public ESimConnect()
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        this.window = new Window();
        var wih = new WindowInteropHelper(window);
        wih.EnsureHandle();
        this.windowHandle = new WindowInteropHelper(this.window).Handle;
      });
    }
    public static void EnsureDllFilesAvailable()
    {
      string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
      path = System.IO.Path.GetDirectoryName(path);

      var firstFile = System.IO.Path.Combine(path, "Microsoft.FlightSimulator.SimConnect.dll");
      var secondFile = System.IO.Path.Combine(path, "SimConnect.dll");

      if (System.IO.File.Exists(firstFile) == false)
        throw new ESimConnectException($"The required dll file '{firstFile}' not found.");

      if (System.IO.File.Exists(secondFile) == false)
        throw new ESimConnectException($"The required dll file '{secondFile}' not found.");
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

    public void Open()
    {
      if (this.simConnect != null)
        throw new InvalidRequestException("SimConnect already opened.");

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
        this.simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;
      }
      catch (Exception ex)
      {
        throw new InternalException("Unable to open connection to FS2020.", ex);
      }
    }

    public void RegisterType<T>(uint typeId) where T : struct
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = (EEnum)typeId;
      int epsilon = 0;

      Type t = typeof(T);
      EnsureTypeHasRequiredAttribute(t);
      var fields = t.GetFields();

      var fieldInfos = fields
        .OrderBy(f => Marshal.OffsetOf(t, f.Name).ToInt32())
        .Select(q => new { Field = q, Attribute = GetDataDefinitionAttributeOrThrowException(q) })
        .Select(q => new
        {
          q.Field,
          q.Attribute.Name,
          q.Attribute.Unit,
          Type = ResolveAttributeType(q.Field, q.Attribute)
        })
        .ToList();

      fieldInfos.ForEach(q => EnsureFieldHasCorrectType(q.Field, q.Type));

      foreach (var fieldInfo in fieldInfos)
      {
        try
        {
          simConnect.AddToDataDefinition(eTypeId,
            fieldInfo.Name, fieldInfo.Unit, fieldInfo.Type,
            epsilon, SimConnect.SIMCONNECT_UNUSED);
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

    public void RequestDataRepeatedly<T>(
      int? customRequestId, SIMCONNECT_PERIOD period, bool sendOnlyOnChange = true, 
      int initialDelayFrames = 0, int skipBetweenFrames = 0, int numberOfReturnedFrames = 0)
    {
      if (this.simConnect == null) throw new NotConnectedException();
      if (initialDelayFrames < 0) initialDelayFrames = 0;
      if (skipBetweenFrames < 0) skipBetweenFrames = 0;
      if (numberOfReturnedFrames < 0) numberOfReturnedFrames = 0;

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

      SIMCONNECT_DATA_REQUEST_FLAG flag = sendOnlyOnChange
        ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
        : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;

      EEnum eRequestId = (EEnum)thisRequestId;
      EEnum eDefineId = (EEnum)rt.Id;
      this.simConnect.RequestDataOnSimObject(eRequestId, eDefineId, SimConnect.SIMCONNECT_OBJECT_ID_USER, period,
        flag, (uint) initialDelayFrames, (uint) skipBetweenFrames, (uint) numberOfReturnedFrames);
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

    private static void EnsureFieldHasCorrectType(FieldInfo field, SIMCONNECT_DATATYPE simType)
    {
      var fieldType = field.FieldType;
      if (fieldType == typeof(string))
      {
        if (simType != SIMCONNECT_DATATYPE.STRING8
          && simType != SIMCONNECT_DATATYPE.STRING32
          && simType != SIMCONNECT_DATATYPE.STRING64
          && simType != SIMCONNECT_DATATYPE.STRING128
          && simType != SIMCONNECT_DATATYPE.STRING256
          && simType != SIMCONNECT_DATATYPE.STRING260)
          throw new InvalidRequestException($"If the field '{field.Name}' is of type string, " +
            $"the expected sim-type should be string too (but declared type is '{simType}'.");

        var marshalAsAttribute = field.GetCustomAttribute<MarshalAsAttribute>() ??
          throw new InvalidRequestException($"If the field '{field.Name}' is of type string, " +
            $"it should have an '[MarshalAs(UnmanagedType.ByValTStr, SizeConst = XXX)]', where XXX is the correct string size.");
        if (marshalAsAttribute.SizeConst == 8 && simType != SIMCONNECT_DATATYPE.STRING8)
          throw new InvalidRequestException($"If the field '{field.Name}' has simType = {simType}, " +
            $"the '[MarshalAs(UnmanagedType.ByValTStr, SizeConst = XXX)]' " +
            $"SizeConst must match (provided value is {marshalAsAttribute.SizeConst}).");
        if (marshalAsAttribute.SizeConst == 32 && simType != SIMCONNECT_DATATYPE.STRING32)
          throw new InvalidRequestException($"If the field '{field.Name}' has simType = {simType}, " +
            $"the '[MarshalAs(UnmanagedType.ByValTStr, SizeConst = XXX)]' " +
            $"SizeConst must match (provided value is {marshalAsAttribute.SizeConst}).");
        if (marshalAsAttribute.SizeConst == 64 && simType != SIMCONNECT_DATATYPE.STRING64)
          throw new InvalidRequestException($"If the field '{field.Name}' has simType = {simType}, " +
            $"the '[MarshalAs(UnmanagedType.ByValTStr, SizeConst = XXX)]' " +
            $"SizeConst must match (provided value is {marshalAsAttribute.SizeConst}).");
        if (marshalAsAttribute.SizeConst == 128 && simType != SIMCONNECT_DATATYPE.STRING128)
          throw new InvalidRequestException($"If the field '{field.Name}' has simType = {simType}, " +
            $"the '[MarshalAs(UnmanagedType.ByValTStr, SizeConst = XXX)]' " +
            $"SizeConst must match (provided value is {marshalAsAttribute.SizeConst}).");
        if (marshalAsAttribute.SizeConst == 256 && simType != SIMCONNECT_DATATYPE.STRING256)
          throw new InvalidRequestException($"If the field '{field.Name}' has simType = {simType}, " +
            $"the '[MarshalAs(UnmanagedType.ByValTStr, SizeConst = XXX)]' " +
            $"SizeConst must match (provided value is {marshalAsAttribute.SizeConst}).");
        if (marshalAsAttribute.SizeConst == 260 && simType != SIMCONNECT_DATATYPE.STRING260)
          throw new InvalidRequestException($"If the field '{field.Name}' has simType = {simType}, " +
            $"the '[MarshalAs(UnmanagedType.ByValTStr, SizeConst = XXX)]' " +
            $"SizeConst must match (provided value is {marshalAsAttribute.SizeConst}).");
      }
      else if (typeMapping.ContainsKey(field.FieldType) && typeMapping[field.FieldType] != simType)
        throw new InvalidRequestException($"The field '{field.Name}' has declared type '{field.FieldType}' but " +
          $"requested sim-type is '{simType}' and should be '{typeMapping[field.FieldType]}'.");
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

    private static IntPtr GetWindowHandle(Window window)
    {
      IntPtr ret = new WindowInteropHelper(window).Handle;
      return ret;
    }

    private DataDefinitionAttribute GetDataDefinitionAttributeOrThrowException(FieldInfo field)
    {
      DataDefinitionAttribute ret = field.GetCustomAttribute<DataDefinitionAttribute>()
        ?? throw new InvalidRequestException($"Field '{field.Name}' has not the " +
        $"required '{nameof(DataDefinitionAttribute)}' attribute.");

      return ret;
    }

    private void RegisterWindowsQueueHandle()
    {
      if (registeredWindowsQueueHandles.Contains(this.windowHandle)) return;

      HwndSource lHwndSource = HwndSource.FromHwnd(this.windowHandle);
      lHwndSource.AddHook(new HwndSourceHook(DefWndProc));

      this.registeredWindowsQueueHandles.Add(this.windowHandle);
    }

    private SIMCONNECT_DATATYPE ResolveAttributeType(FieldInfo field, DataDefinitionAttribute att)
    {
      SIMCONNECT_DATATYPE ret;

      if (att.Type != SimType.UNSPECIFIED)
        ret = att.TypeAsSimConnectDataType;
      else if (typeMapping.ContainsKey(field.FieldType))
        ret = typeMapping[field.FieldType];
      else if (field.FieldType == typeof(string))
      {
        var marshalAsAttribute = field.GetCustomAttribute<MarshalAsAttribute>() ??
          throw new InvalidRequestException($"The field '{field.Name}' is of type string, but " +
          $"it has not custom type defined and also [MarshalAsAttribute] is missing to " +
          $"resolve the correct string length.");
        if (typeStringMapping.TryGetValue(marshalAsAttribute.SizeConst, out ret) == false)
          throw new InvalidRequestException($"The field '{field.Name}' is of type string," +
            $"but its [MarshallAsAttribute].SizeConst doest not match predefined " +
            $"string sizes (8/32/64/128/256/260).");
      }
      else
      {
        throw new InvalidRequestException($"The field '{field.Name}' has defined type " +
          $"'{field.FieldType}', which has not predefined mapping to SIMCONNECT_DATATYPE. " +
          $"you must define the 'type' parameter in [{nameof(DataDefinitionAttribute)}] attribute.");
      }

      return ret;
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

    private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
      int iRequest = (int)data.dwRequestID;
      object ret = data.dwData[0];

      int? userRequestId = requestIds.ContainsKey(iRequest) ? requestIds[iRequest] : null;

      ESimConnectDataReceivedEventArgs e = new(userRequestId, ret);
      this.DataReceived?.Invoke(this, e);
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
