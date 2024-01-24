using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Sustainers
{
    internal class SetFailureSustainer : SimVarBasedFailureSustainer
    {
        #region Private Fields

        private readonly SetFailureDefinition failure;

        #endregion Private Fields

        #region Public Constructors

        public SetFailureSustainer(SetFailureDefinition failure) : base(failure)
        {
            this.failure = failure;
        }

        #endregion Public Constructors

        #region Protected Methods

        protected override void InitInternal()
        {
            base.InitInternal();
        }

        protected override void ResetInternal()
        {
            SendEvent(failure.OkValue); // Expected to be 0 typically
        }

        protected override void StartInternal()
        {
            SendEvent(failure.FailValue); // Expected to be 1 typically
        }

        #endregion Protected Methods

        #region Private Methods

        private void SendEvent(double arg)
        {
            SendData(arg);
        }

        #endregion Private Methods
    }
}
