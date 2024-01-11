using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Incidents
{
    public struct Percentage
    {
        private double value;

        public Percentage(double value)
        {
            this.value = EnsureInRange(value);
        }

        private static double EnsureInRange(double value)
        {
            return Math.Min(Math.Max(value, 0), 1);
        }

        public static explicit operator Percentage(double value) => new Percentage(value);

        public static implicit operator double(Percentage value) => value.value;

        public override string ToString()
        {
            return (value * 100) + "%";
        }
    }
}
