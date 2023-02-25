using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESimConnect.Types
{
  internal class RegisteredTypeManager
  {
    private class RegisteredType
    {
      public int id;
      public Type type;

      public RegisteredType(int id, Type type)
      {
        this.id = id;
        this.type = type ?? throw new ArgumentNullException(nameof(type));
      }
    }

    private readonly List<RegisteredType> inner = new();
    public void Register(int id, Type type)
    {
      if (inner.Any(q => q.id == id))
        throw new InvalidRequestException(
          $"Unable to register type. ID '{id}' already registered with type '{GetType(id)}'.");
      inner.Add(new RegisteredType(id, type));
    }

    internal Type GetType(int id)
    {
      RegisteredType rt = inner.FirstOrDefault(q => q.id == id)
        ?? throw new InvalidRequestException($"ID '{id}' has no registered type.");
      Type ret = rt.type;
      return ret;
    }

    internal int GetId(Type type)
    {
      RegisteredType rt = inner.FirstOrDefault(q => q.type == type)
        ?? throw new InvalidRequestException($"Type '{type.Name}' has not been registered.");
      int ret = rt.id;
      return ret;
    }

    internal EEnum GetEnumId(Type type) => (EEnum)this.GetId(type);

    internal void Unregister(Type type)
    {
      inner.Remove(inner.Single(q => q.type == type));
    }
  }
}
