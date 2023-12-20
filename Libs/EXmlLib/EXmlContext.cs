using EXmlLib.Deserializers;
using EXmlLib.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EXmlLib
{
  public class EXmlContext
  {
    private class DefaultObjectFactoryImplementation : IFactory
    {
      public bool AcceptsType(Type type)
      {
        return true;
      }

      public object CreateInstance(Type targetType)
      {
        object ret;
        try
        {
          var ctor = targetType.GetConstructor(Array.Empty<Type>());
          ret = ctor!.Invoke(Array.Empty<object>());
        }
        catch (Exception ex)
        {
          throw new EXmlException(
            $"Unable to find or invoke public parameterless constructor for type '{targetType}'.",
            ex);
        }
        return ret;
      }
    }

    public class ContextCustomData
    {
      private readonly Dictionary<string, object> data = new();
      public object this[string key]
      {
        get
        {
          if (data.ContainsKey(key))
          {
            return data[key];
          }
          else
            throw new ArgumentException($"There is no key 'key' in CustomData.");
        }
        set
        {
          this.data[key] = value;
        }
      }

      public T Get<T>(string key)
      {
        T ret;
        object tmp = this[key];
        if (tmp is T)
          ret = (T)tmp;
        else
          throw new ArgumentException($"The CustomData item '{key}' is not of the type '{typeof(T).Name}' (but '{tmp.GetType().Name}')");
        return ret;
      }

      public void Delete(string key)
      {
        if (data.ContainsKey(key))
          data.Remove(key);
      }
    }

    private DefaultObjectFactoryImplementation _DefaultObjectFactory = new();
    public List<IAttributeDeserializer> AttributeDeserializers { get; private set; } = new();
    public IFactory DefaultObjectFactory => _DefaultObjectFactory;
    public List<IElementDeserializer> ElementDeserializers { get; private set; } = new();
    public List<IFactory> Factories { get; private set; } = new();
    public bool IgnoreMissingProperties { get; set; } = true;
    public ContextCustomData CustomData { get; } = new ContextCustomData();

    public IAttributeDeserializer ResolveAttributeDeserializer(Type type)
    {
      return this.AttributeDeserializers.FirstOrDefault(q => q.AcceptsType(type))
        ?? throw new EXmlException($"Attribute deserializer for type '{type}' not found.");
    }

    public IAttributeDeserializer? TryResolveAttributeDeserializer(Type type)
    {
      return this.AttributeDeserializers.FirstOrDefault(q => q.AcceptsType(type));
    }

    public IElementDeserializer ResolveElementDeserializer(Type type)
    {
      return this.ElementDeserializers.First(q => q.AcceptsType(type))
        ?? throw new EXmlException($"Element deserializer for type '{type}' not found.");
    }

    public IElementDeserializer? TryResolveElementDeserializer(Type type)
    {
      return this.ElementDeserializers.FirstOrDefault(q => q.AcceptsType(type));
    }

    public IFactory ResolveFactory(Type type)
    {
      return this.Factories.FirstOrDefault(q => q.AcceptsType(type))
        ?? throw new EXmlException($"Factory for type '{type}' not found.");
    }

    public IFactory? TryResolveFactory(Type type)
    {
      return this.Factories.FirstOrDefault(q => q.AcceptsType(type));
    }

    public string ResolvePropertyName(PropertyInfo prop)
    {
      return prop.Name;
    }
  }
}
