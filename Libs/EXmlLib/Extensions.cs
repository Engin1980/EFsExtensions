using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXmlLib
{
  public static class Extensions
  {
    public static XElement LElement(this XElement element, string name)
    {
      return element.Elements().First(q => q.Name.LocalName == name);
    }

    public static XElement? LElementOrNull(this XElement element, string name)
    {
      return element.Elements().FirstOrDefault(q => q.Name.LocalName == name);
    }

    public static IEnumerable<XElement> LElements(this XElement element, string name)
    {
      return element.Elements().Where(q => q.Name.LocalName == name);
    }
  }
}
