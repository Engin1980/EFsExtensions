using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ESimConnect.Types
{
  internal class RequestIdManager
  {
    private readonly Dictionary<EEnum, int> inner = new();

    public void Register(int? customId, EEnum requestId)
    {
      if (customId != null)
        if (inner.Values.Any(q => q == customId))
          throw new InvalidRequestException($"customRequestId '{customId}' is already registered.");
        else
          inner[requestId] = customId.Value;
    }

    public void Unregister(int? customId)
    {
      if (!inner.Values.Any(q => q == customId))
        inner.Remove(inner.Single(q => q.Value == customId).Key);
    }

    public int? Recall(EEnum requestId)
    {
      int? ret;
      if (inner.ContainsKey(requestId))
        ret = inner[requestId];
      else
        ret = null;
      return ret;
    }
  }
}
