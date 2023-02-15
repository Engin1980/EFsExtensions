using EXmlLib.Deserializers;
using System.Reflection;
using System.Xml.Linq;

namespace EXmlLib
{
  public class EXml<T>
  {
    public EXmlContext Context { get; private set; } = new();

    public EXml()
    {
      Context.ElementDeserializers.Add(new EnumDeserializer());
      Context.ElementDeserializers.Add(new NumberDeserializer());
      Context.ElementDeserializers.Add(new StringDeserializer());
      Context.ElementDeserializers.Add(new ObjectElementDeserializer());

      Context.AttributeDeserializers.Add(new EnumDeserializer());
      Context.AttributeDeserializers.Add(new NumberDeserializer());
      Context.AttributeDeserializers.Add(new StringDeserializer());
    }

    public T Deserialize(XDocument doc)
    {
      T ret = this.Deserialize(doc.Root);
      return ret;
    }

    public T Deserialize(XElement element)
    {
      object tmp;

      IElementDeserializer elementDeserializer = Context.ResolveElementDeserializer(typeof(T))
        ?? throw new EXmlException($"Unable to find element deserializer for type '{typeof(T)}'.");
      tmp = SafeUtils.Deserialize(element, typeof(T), elementDeserializer, Context);
      T ret = SafeUtils.Cast<T>(tmp);
      return ret;
    }

  }
}