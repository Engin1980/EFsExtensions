using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXmlLib.Factories
{
  public interface IFactory
  {
    public bool AcceptsType(Type type);

    public object CreateInstance(Type targetType);
  }
}
