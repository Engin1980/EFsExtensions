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

      public ESimConnectDataReceivedEventArgs(int? requestId, object data)
      {
        RequestId = requestId;
        Data = data ?? throw new ArgumentNullException(nameof(data));
      }
    }
    public class ESimConnectEventInvokedEventArgs
    {
      public ESimConnectEventInvokedEventArgs(int? requestId, uint value)
      {
        RequestId = requestId;
        Value = value;
      }

      public int? RequestId { get; set; }
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
    private readonly RegisteredTypeManager typeManager = new();
    private readonly WinHandleManager winHandleManager = new();
    private readonly RequestIdManager requestIdManager = new();
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

    public void RegisterEvent(int eventId, string eventName)
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eRequestId = RequestIdProvider.GetNextEnum();
      try
      {
        this.simConnect.SubscribeToSystemEvent(eRequestId, eventName);
        requestIdManager.Register(eventId, eRequestId);
      }
      catch (Exception ex)
      {
        throw new InternalException($"Failed to register sim-event listener for '{eventName}'.", ex);
      }
    }

    public void RegisterType<T>(int typeId) where T : struct
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = (EEnum)typeId;
      int epsilon = 0;

      Type t = typeof(T);
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
        this.typeManager.Register(typeId, t);
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

      Type t = typeof(T);
      int typeId = typeManager.GetId(t);

      EEnum eRequestId = RequestIdProvider.GetNextEnum();
      EEnum eDefineId = (EEnum)typeId;
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

      SIMCONNECT_DATA_REQUEST_FLAG flag = sendOnlyOnChange
        ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
        : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;

      EEnum eDefineId = typeManager.GetEnumId(typeof(T));
      EEnum eRequestId = RequestIdProvider.GetNextEnum();
      

      try
      {
        this.simConnect.RequestDataOnSimObject(eRequestId, eDefineId, SimConnect.SIMCONNECT_OBJECT_ID_USER, period,
          flag, (uint)initialDelayFrames, (uint)skipBetweenFrames, (uint)numberOfReturnedFrames);
        this.requestIdManager.Register(customRequestId, eRequestId);
      }
      catch (Exception ex)
      {
        throw new InternalException($"Failed to invoke 'RequestDataOnSimObject(...)'.", ex);
      }
    }

    public void UnregisterType<T>()
    {
      if (this.simConnect == null) throw new NotConnectedException();

      EEnum eTypeId = typeManager.GetEnumId(typeof(T));

      try { 
      this.simConnect.ClearDataDefinition(eTypeId);
        this.typeManager.Unregister(typeof(T));
      } catch (Exception ex)
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
      EEnum iRequest = (EEnum)data.dwRequestID;
      object ret = data.dwData[0];

      int? userRequestId = requestIdManager.Recall(iRequest);

      ESimConnectDataReceivedEventArgs e = new(userRequestId, ret);
      this.DataReceived?.Invoke(this, e);
    }
    private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
    {
      EEnum iRequest = (EEnum)data.dwRequestID;
      int? userRequestId = requestIdManager.Recall(iRequest);
      object ret = data.dwData[0];

      ESimConnectDataReceivedEventArgs e = new(userRequestId, ret);
      this.DataReceived?.Invoke(this, e);
    }

    private void SimConnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
    {
      EEnum iRequest = (EEnum)data.dwID;
      int? userRequestId = requestIdManager.Recall(iRequest);
      uint value = data.dwData;

      ESimConnectEventInvokedEventArgs e = new(userRequestId, value);
      this.EventInvoked?.Invoke(this, e);
    }
  }
}
