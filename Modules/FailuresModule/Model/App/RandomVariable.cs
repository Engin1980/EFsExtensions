using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.App
{
    public class RandomVariable : Variable
    {
        private static readonly Random rnd = new();
        public double Minimum { get; set; }
        public double Maximum { get; set; }

        private double? _Value = null;
        public override double Value
        {
            get
            {
                if (_Value == null)
                    _Value = Minimum + rnd.NextDouble() * (Maximum - Minimum);
                return _Value.Value;
            }
        }
    }
}
