using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.App
{
    public class UserVariable : Variable
    {
        public double DefaultValue { get; set; }
        public double? UserValue { get; set; }
        public override double Value { get => UserValue ?? DefaultValue; }
    }
}
