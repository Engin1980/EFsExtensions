using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib.Deserializers
{
  public interface IElementDeserializer
  {
    public bool AcceptsType(Type type);
    public object Deserialize(XElement element, Type targetType, EXmlContext context);
  }
}
