using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Failures;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Sustainers
{
    public abstract class FailureSustainer : NotifyPropertyChanged
    {
        #region Fields

        private static NewSimObject eSimObj = null!;
        private bool initialized = false;

        #endregion Fields

        #region Properties

        public FailureDefinition Failure { get; }

        public bool IsActive
        {
            get => GetProperty<bool>(nameof(IsActive))!;
            private set => UpdateProperty(nameof(IsActive), value);
        }
        protected NewSimObject ESimObj { get => eSimObj; }

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
            if (eSimObj == null) throw new ApplicationException("eSimObj is null.");
            if (IsActive)
            {
                ResetInternal();
                IsActive = false;
            }
        }

        public void Start()
        {
            if (eSimObj == null) throw new ApplicationException("eSimObj is null.");
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

        internal static void SetSimCon(NewSimObject eSimObj)
        {
            FailureSustainer.eSimObj = eSimObj;
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
