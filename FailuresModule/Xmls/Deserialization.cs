using ELogging;
using EXmlLib;
using EXmlLib.Deserializers;
using FailuresModule.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FailuresModule.Xmls
{
  public class Deserialization
  {
    private const string FAILURES_KEY = "__failures";
    private const string LOG_HANDLER_KEY = "__log_handler";
    public static EXml<FailGroup> CreateDeserializer(List<Failure> failures, NewLogHandler logHandler)
    {
      EXml<FailGroup> ret = new();

      Dictionary<string, Failure> failDict = failures.ToDictionary(q => q.Id, q => q);
      ret.Context.CustomData[FAILURES_KEY] = failDict;
      ret.Context.CustomData[LOG_HANDLER_KEY] = logHandler;
      ret.Context.ElementDeserializers.Insert(0, CreateFailGroupDeserializer());

      return ret;
    }

    private static IElementDeserializer CreateFailGroupDeserializer()
    {
      ObjectElementDeserializer ret = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(FailGroup))
        .WithCustomPropertyDeserialization(nameof(FailGroup.Groups),
          (e, t, p, c) =>
          {
            var deser = c.ResolveElementDeserializer(typeof(FailGroup));
            var items = e.LElements("group")
              .Select(q => SafeUtils.Deserialize(q, typeof(FailGroup), deser, c))
              .Cast<FailGroup>()
              .ToList();
            var bindingItems = new BindingList<FailGroup>(items);
            SafeUtils.SetPropertyValue(p, t, bindingItems);
          })
        .WithCustomPropertyDeserialization(nameof(FailGroup.Failures),
          (e, t, p, c) =>
          {
            IEnumerable<string> ids = e.LElements("failure").Select(q => q.Attribute("id")!.Value);
            Dictionary<string, Failure> failures =
              c.CustomData.Get<Dictionary<string, Failure>>(FAILURES_KEY);
            NewLogHandler newLogHandler = c.CustomData.Get<NewLogHandler>(LOG_HANDLER_KEY);
            BindingList<Failure> items = new();
            foreach (var id in ids)
            {
              if (!failures.TryGetValue(id, out Failure? f))
                newLogHandler.Invoke(LogLevel.WARNING, "Unknown failure id '{id}'. Skipped.");
              else
                items.Add(f!);
            }

            SafeUtils.SetPropertyValue(p, t, items);
          });

      return ret;
    }
  }
}
