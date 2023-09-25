//using Eng.Chlaot.ChlaotModuleBase;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FailuresModule.Types.Old
//{
//    public abstract class FailureFrequency : NotifyPropertyChangedBase
//    {
//    }

//    public class ProbabilityFailureFrequency : FailureFrequency
//    {
//        public int Probability
//        {
//            get => GetProperty<int>(nameof(Probability))!;
//            set => UpdateProperty(nameof(Probability), value);
//        }
//    }

//    public class MtbfFailureFrequency : FailureFrequency
//    {

//        public double MTBF
//        {
//            get => GetProperty<double>(nameof(MTBF))!;
//            set
//            {
//                UpdateProperty(nameof(MTBF), value);
//                CalculatedProbability = 100d / value;
//            }
//        }


//        public double CalculatedProbability
//        {
//            get => GetProperty<double>(nameof(CalculatedProbability))!;
//            set => UpdateProperty(nameof(CalculatedProbability), value);
//        }
//    }

//}
