using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESimConnect
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
  internal class SimDeprecatedAttribute : Attribute
  {
  }
}
