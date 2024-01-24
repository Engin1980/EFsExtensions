using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Sustainers
{
    internal class LeakFailureSustainer : SimVarBasedFailureSustainer
    {
        #region Fields

        private readonly int expectedNumberOfTicksBeforeLeakOut;
        private int simSecondElapsedEventId;
        private readonly LeakFailureDefinition failure;

        #endregion Fields

        #region Properties

        public double? CurrentValue
        {
            get => GetProperty<double?>(nameof(CurrentValue))!;
            private set => UpdateProperty(nameof(CurrentValue), value);
        }

        public double? InitialValue
        {
            get => GetProperty<double?>(nameof(InitialValue))!;
            private set => UpdateProperty(nameof(InitialValue), value);
        }

        public double? LeakPerTick
        {
            get => GetProperty<double?>(nameof(LeakPerTick))!;
            private set => UpdateProperty(nameof(LeakPerTick), value);
        }

        #endregion Properties

        #region Constructors

        public LeakFailureSustainer(LeakFailureDefinition failure) : base(failure)
        {
            this.failure = failure;

            expectedNumberOfTicksBeforeLeakOut = new Random().Next(failure.MinimumLeakTicks, failure.MaximumLeakTicks);
            ResetInternal();
            DataReceived += LeakFailureSustainer_DataReceived;
            SimCon.EventInvoked += SimCon_EventInvoked;
            //TODO not using custom refresh leak-tick-ms interval
        }

        #endregion Constructors

        #region Methods

        protected override void InitInternal()
        {
            base.InitInternal();
            simSecondElapsedEventId = SimCon.RegisterSystemEvent(ESimConnect.SimEvents.System._1sec);
        }

        protected override void ResetInternal()
        {
            LeakPerTick = CurrentValue = InitialValue = null;
        }

        protected override void StartInternal()
        {
            RequestData();
        }

        private void ApplyLeak()
        {
            CurrentValue -= LeakPerTick;
            if (CurrentValue < 0) CurrentValue = 0;
            SendData(CurrentValue!.Value);
        }

        private void LeakFailureSustainer_DataReceived(double value)
        {
            if (InitialValue == null)
            {
                lock (this)
                {
                    if (InitialValue == null)
                    {
                        InitialValue = CurrentValue = value;
                        LeakPerTick = CurrentValue / expectedNumberOfTicksBeforeLeakOut;
                    }
                }
            }
        }

        private void SimCon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
        {
            if (e.RequestId == simSecondElapsedEventId && CurrentValue != null && IsSimPaused == false)
                ApplyLeak();
        }

        #endregion Methods
    }
}
