using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Sustainers
{
    internal class StuckFailureSustainer : SimVarBasedFailureSustainer
    {
        #region Fields

        private bool isRunning = false;
        private readonly Timer updateTimer;
        private StuckFailureDefinition failure;

        #endregion Fields

        #region Properties

        public double? StuckValue
        {
            get => GetProperty<double?>(nameof(StuckValue))!;
            set => UpdateProperty(nameof(StuckValue), value);
        }

        #endregion Properties

        #region Constructors

        public StuckFailureSustainer(StuckFailureDefinition failure) : base(failure)
        {
            this.failure = failure;
            updateTimer = new Timer(this.failure.RefreshIntervalInMs);
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            DataReceived += StuckFailureSustainer_DataReceived;
            //TODO is not using "onlyWhenChanged" flag
        }

        #endregion Constructors

        #region Methods

        protected override void InitInternal()
        {
            base.InitInternal();
        }

        protected override void ResetInternal()
        {
            lock (this)
            {
                updateTimer.Enabled = false;
                isRunning = false;
                StuckValue = null;
            }
        }

        protected override void StartInternal()
        {
            isRunning = true;
            RequestData();
        }

        private void StuckFailureSustainer_DataReceived(double data)
        {
            if (StuckValue == null && isRunning)
            {
                lock (this)
                {
                    if (StuckValue == null)
                    {
                        StuckValue = data;
                        updateTimer.Start();
                    }
                }
            }
        }
        private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                Debug.Assert(StuckValue != null);
                SendData(StuckValue.Value);
            }
        }

        #endregion Methods
    }
}
