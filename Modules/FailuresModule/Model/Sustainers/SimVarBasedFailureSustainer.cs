using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Sustainers
{
    public abstract class SimVarBasedFailureSustainer : FailureSustainer
    {
        // taken from https://github.com/kanaron/RandFailuresFS2020/blob/ab2cb278df8ede6739bcfe60a7f34c9f97b8f5ba/RandFailuresFS2020/RandFailuresFS2020/Simcon.cs
        private const string DEFAULT_UNIT = "Number";
        private const string DEFAULT_TYPE = "FLOAT64";
        private bool isRegistered = false;
        private int typeId;
        private int requestId = -1;
        protected bool IsSimPaused { get; private set; } = false;
        protected event Action<double>? DataReceived;

        protected SimVarBasedFailureSustainer(FailureDefinition failure) : base(failure)
        {
            SimCon.EventInvoked += SimCon_EventInvoked;
        }

        protected override void InitInternal()
        {
            SimCon.RegisterSystemEvent(ESimConnect.SimEvents.System.Paused);
            SimCon.RegisterSystemEvent(ESimConnect.SimEvents.System.Unpaused);
        }

        private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
        {
            if (e.Event == ESimConnect.SimEvents.System.Paused)
                IsSimPaused = true;
            else if (e.Event == ESimConnect.SimEvents.System.Unpaused)
                IsSimPaused = false;
        }

        private void RegisterIfRequired()
        {
            if (isRegistered) return;

            SimCon.DataReceived += SimCon_DataReceived;

            string name = Failure.SimConPoint;
            typeId = SimCon.RegisterPrimitive<double>(name, DEFAULT_UNIT, DEFAULT_TYPE);
            isRegistered = true;
        }

        protected void RequestData()
        {
            RegisterIfRequired();
            SimCon.RequestPrimitive(typeId, out requestId);
        }

        private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
        {
            if (e.RequestId == requestId)
            {
                double data = (double)e.Data;
                DataReceived?.Invoke(data);
            }
        }

        internal void SendData(double value)
        {
            RegisterIfRequired();
            SimCon.SendPrimitive(typeId, value);
        }

        internal void RequestDataRepeatedly()
        {
            RegisterIfRequired();
            SimCon.RequestPrimitiveRepeatedly(typeId, out requestId, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_PERIOD.SECOND);
        }
    }
}
