using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Sustainers
{
    public abstract class FailureSustainer : NotifyPropertyChangedBase
    {
        #region Fields

        private static ESimConnect.ESimConnect simCon = null!;
        private bool initialized = false;

        #endregion Fields

        #region Properties

        public FailureDefinition Failure { get; }

        public bool IsActive
        {
            get => GetProperty<bool>(nameof(IsActive))!;
            private set => UpdateProperty(nameof(IsActive), value);
        }
        protected ESimConnect.ESimConnect SimCon { get => simCon; }

        #endregion Properties

        #region Constructors

        protected FailureSustainer(FailureDefinition failure)
        {
            Failure = failure ?? throw new ArgumentNullException(nameof(failure));
        }

        #endregion Constructors

        #region Methods

        public void Reset()
        {
            if (SimCon == null) throw new ApplicationException("SimCon is null.");
            if (IsActive)
            {
                ResetInternal();
                IsActive = false;
            }
        }

        public void Start()
        {
            if (SimCon == null) throw new ApplicationException("SimCon is null.");
            if (!initialized)
                Init();

            if (!IsActive)
            {
                StartInternal();
                IsActive = true;
            }
        }

        public void Toggle()
        {
            if (!IsActive)
                Start();
            else
                Reset();
        }

        internal static void SetSimCon(ESimConnect.ESimConnect eSimConnect)
        {
            simCon = eSimConnect;
        }

        protected abstract void InitInternal();

        protected abstract void ResetInternal();

        protected abstract void StartInternal();

        private void Init()
        {
            InitInternal();
            initialized = true;
        }

        #endregion Methods
    }
}
