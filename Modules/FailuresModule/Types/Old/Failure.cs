using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.Old
{
    public class Failure : NotifyPropertyChangedBase
    {

        public FailureDefinition Definition
        {
            get => GetProperty<FailureDefinition>(nameof(Definition))!;
            set => UpdateProperty(nameof(Definition), value);
        }


        public FailureFrequency Frequency
        {
            get => GetProperty<FailureFrequency>(nameof(Frequency))!;
            set => UpdateProperty(nameof(Frequency), value);
        }
    }
}
