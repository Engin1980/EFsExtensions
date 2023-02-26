using ESimConnect.Types;
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
      public Type Type { get; set; }

      public ESimConnectDataReceivedEventArgs(int? requestId, Type type, object data)
      {
        this.RequestId = requestId;
        this.Type = type ?? throw new ArgumentNullException(nameof(type));
        this.Data = data ?? throw new ArgumentNullException(nameof(data));
      }
    }
    public class ESimConnectEventInvokedEventArgs
    {
      public ESimConnectEventInvokedEventArgs(string @event, uint value)
      {
        Event = @event;
        Value = value;
      }

      public string Event { get; set; }
      public uint Value { get; set; }

    }
    #region Delegates
    public delegate void ESimConnectDataReceived(ESimConnect sender, ESimConnectDataReceivedEventArgs e);
    public delegate void ESimConnectEventInvoked(ESimConnect sender, ESimConnectEventInvokedEventArgs e);

    public delegate void ESimConnectDelegate(ESimConnect sender);

    public delegate void ESimConnectExceptionDelegate(ESimConnect sender, SIMCONNECT_EXCEPTION ex);
    #endregion
    #region Events
    public event ESimConnectDelegate? Connected;

    public event ESimConnectDataReceived? DataReceived;

    public event ESimConnectEventInvoked? EventInvoked;

    public event ESimConnectDelegate? Disconnected;

    public event ESimConnectExceptionDelegate? ThrowsException;
    #endregion
    private readonly TypeManager typeManager = new();
    private readonly Types.EventManager eventManager = new();
    private readonly WinHandleManager winHandleManager = new();
    private readonly RequestManager requestIdManager = new();
    private SimConnect? simConnect;
    public bool IsOpened { get => this.simConnect != null; }

    public ESimConnect()
    {
      winHandleManager.Acquire();
      winHandleManager.FsExitDetected += (() => ResolveExitedFS2020());
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
      this.winHandleManager.Dispose();
    }

    public void Open()
    {
      if (this.simConnect != null)
        throw new InvalidRequestException("SimConnect already opened.");

      try
      {
        winHandleManager.Acquire();
      }
      catch (Exception ex)
      {
        throw new InternalException("Failed to register windows queue handler.", ex);
      }

      try
      {
        this.simConnect = new SimConnect("ESimConnect", winHandleManager.Handle, WinHandleManager.WM_USER_SIMCONNECT, null, 0);
        this.simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
        this.simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
        this.simConnect.OnRecvException += SimConnect_OnRecvException;
        this.simConnect.OnRecvSimobjectDataBytype += SimConnect_OnRecvSimobjectDataBytype;
        this.simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;
        this.simConnect.OnRecvEvent += SimConnect_OnRecvEvent;
        this.winHandleManager.Acquire();
        this.winHandleManager.SimConnect = this.simConnect;
      }
      catch (Exception ex)
      {
        throw new InternalException("Unable to open connection to FS2020.", ex);
      }
    }

    public void RegisterSystemEvent(string eventName)
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eRequestId = IdProvider.GetNextAsEnum();
      try
      {
        this.simConnect.SubscribeToSystemEvent(eRequestId, eventName);
        this.eventManager.Register(eRequestId, eventName);
      }
      catch (Exception ex)
      {
        throw new InternalException($"Failed to register sim-event listener for '{eventName}'.", ex);
      }
    }

    public void RegisterType<T>() where T : struct
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = IdProvider.GetNextAsEnum();
      int epsilon = 0;

      Type t = typeof(T);
      //TODO refactor to method:
      SanityHelpers.EnsureTypeHasRequiredAttribute(t);
      var fields = t.GetFields();

      var fieldInfos = fields
        .OrderBy(f => Marshal.OffsetOf(t, f.Name).ToInt32())
        .Select(q => new { Field = q, Attribute = SanityHelpers.GetDataDefinitionAttributeOrThrowException(q) })
        .Select(q => new
        {
          q.Field,
          q.Attribute.Name,
          q.Attribute.Unit,
          Type = SanityHelpers.ResolveAttributeType(q.Field, q.Attribute)
        })
        .ToList();

      fieldInfos.ForEach(q => SanityHelpers.EnsureFieldHasCorrectType(q.Field, q.Type));

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
        this.typeManager.Register((int)eTypeId, t);
      }
      catch (Exception ex)
      {
        throw new InternalException("Failed to invoke 'simConnect.RegisterDataDefineStruct<T>(...)'.", ex);
      }
    }

    public void RequestData<T>(
          int? customRequestId = null, uint radius = 0,
      SIMCONNECT_SIMOBJECT_TYPE simObjectType = SIMCONNECT_SIMOBJECT_TYPE.USER)
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = typeManager.GetIdAsEnum(typeof(T));
      EEnum eRequestId = IdProvider.GetNextAsEnum();
      this.simConnect.RequestDataOnSimObjectType(eRequestId, eTypeId, radius, simObjectType);
      requestIdManager.Register(customRequestId, typeof(T), eRequestId);
    }

    public void RequestDataRepeatedly<T>(
      int? customRequestId, SIMCONNECT_PERIOD period, bool sendOnlyOnChange = true,
      int initialDelayFrames = 0, int skipBetweenFrames = 0, int numberOfReturnedFrames = 0)
    {
      if (this.simConnect == null) throw new NotConnectedException();
      if (initialDelayFrames < 0) initialDelayFrames = 0;
      if (skipBetweenFrames < 0) skipBetweenFrames = 0;
      if (numberOfReturnedFrames < 0) numberOfReturnedFrames = 0;

      System.IO.File.AppendAllText("logo.txt", "a");

      SIMCONNECT_DATA_REQUEST_FLAG flag = sendOnlyOnChange
        ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
        : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;

      System.IO.File.AppendAllText("logo.txt", "b");
      EEnum eTypeId =  typeManager.GetIdAsEnum(typeof(T));
      EEnum eRequestId = IdProvider.GetNextAsEnum();

      System.IO.File.AppendAllText("logo.txt", "c");
      try
      {
        System.IO.File.AppendAllText("logo.txt", "d");
        this.simConnect.RequestDataOnSimObject(eRequestId, eTypeId, SimConnect.SIMCONNECT_OBJECT_ID_USER, period,
          flag, (uint)initialDelayFrames, (uint)skipBetweenFrames, (uint)numberOfReturnedFrames);
        System.IO.File.AppendAllText("logo.txt", "e");
        this.requestIdManager.Register(customRequestId, typeof(T), eRequestId);
        System.IO.File.AppendAllText("logo.txt", "F");
      }
      catch (Exception ex)
      {
        System.IO.File.AppendAllText("logo.txt", "g");
        throw new InternalException($"Failed to invoke 'RequestDataOnSimObject(...)'.", ex);
      }
      System.IO.File.AppendAllText("logo.txt", "i");
    }

    public void UnregisterType<T>()
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = typeManager.GetIdAsEnum(typeof(T));

      try
      {
        this.simConnect.ClearDataDefinition(eTypeId);
        this.typeManager.Unregister(typeof(T));
      }
      catch (Exception ex)
      {
        throw new InternalException($"Failed to unregister type {typeof(T).Name}.", ex);
      }
    }

    private void ResolveExitedFS2020()
    {
      if (this.simConnect != null)
      {
        this.simConnect.Dispose();
        this.simConnect = null;
      }
      this.Disconnected?.Invoke(this);
    }

    private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
    {
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvException");

      SIMCONNECT_EXCEPTION ex = (SIMCONNECT_EXCEPTION)data.dwException;
      ThrowsException?.Invoke(this, ex);
    }
    private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvOpen");

      this.Connected?.Invoke(this);
    }
    private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
    {
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvQuit");

      this.Disconnected?.Invoke(this);
    }
    private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvSimobjectData\n");

      EEnum iRequest = (EEnum)data.dwRequestID;
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvSimobjectData" + iRequest + "\n");
      object ret = data.dwData[0];
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvSimobjectData a" + iRequest + "\n");
      requestIdManager.Recall(iRequest, out Type type, out int? userRequestId);
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvSimobjectData b" + iRequest + "\n");
      ESimConnectDataReceivedEventArgs e = new(userRequestId, type, ret);
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvSimobjectData c" + iRequest + "\n");
      this.DataReceived?.Invoke(this, e);
    }
    private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
    {
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvSimobjectDataBytype");
      EEnum iRequest = (EEnum)data.dwRequestID;
      requestIdManager.Recall(iRequest, out Type type, out int? userRequestId);
      object ret = data.dwData[0];

      ESimConnectDataReceivedEventArgs e = new(userRequestId, type, ret);
      this.DataReceived?.Invoke(this, e);
    }

    private void SimConnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
    {
      System.IO.File.AppendAllText("logo.txt", "SimConnect_OnRecvEvent");
      EEnum iRequest = (EEnum)data.dwID;
      string @event = eventManager.GetEvent(iRequest);
      uint value = data.dwData;

      ESimConnectEventInvokedEventArgs e = new(@event, value);
      this.EventInvoked?.Invoke(this, e);
    }
  }
}
