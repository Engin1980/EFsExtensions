using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.App
{
    public class Trigger
    {
        public Percentage Probability { get; set; }
        public bool Repetitive { get; set; }
        public IStateCheckItem Condition { get; set; }
    }
}
