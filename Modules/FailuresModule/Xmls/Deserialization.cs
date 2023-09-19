using ELogging;
using EXmlLib;
using EXmlLib.Deserializers;
using FailuresModule.Types.Old;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;

namespace FailuresModule.Xmls
{
    public class Deserialization
  {
    private const string FAILURES_KEY = "__failures";
    private const string LOG_HANDLER_KEY = "__log_handler";
    public static EXml<FailGroup> CreateDeserializer(List<FailureDefinition> failures, NewLogHandler logHandler)
    {
      EXml<FailGroup> ret = new();

      Dictionary<string, FailureDefinition> failDict = failures.ToDictionary(q => q.Id, q => q);
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
        .WithCustomPropertyDeserialization(nameof(FailGroup.Frequency),
        (e, t, p, c) =>
        {
          NewLogHandler newLogHandler = c.CustomData.Get<NewLogHandler>(LOG_HANDLER_KEY);
          var frequency = LoadFrequency($"Group {e.Attribute("title")!.Value}", e, newLogHandler);
          SafeUtils.SetPropertyValue(p, t, frequency);
        })
        .WithCustomPropertyDeserialization(nameof(FailGroup.Failures),
          (e, t, p, c) =>
          {
            var tmps = e.LElements("failure").Select(q => new
            {
              Id = q.Attribute("id")!.Value,
              Element = q
            });
            Dictionary<string, FailureDefinition> failures =
              c.CustomData.Get<Dictionary<string, FailureDefinition>>(FAILURES_KEY);
            NewLogHandler newLogHandler = c.CustomData.Get<NewLogHandler>(LOG_HANDLER_KEY);
            BindingList<Failure> items = new();
            foreach (var tmp in tmps)
            {
              if (!failures.TryGetValue(tmp.Id, out FailureDefinition? f))
              {
                newLogHandler.Invoke(LogLevel.WARNING, "Unknown failure id '{id}'. Skipped.");
                continue;
              }
              Failure failure = new()
              {
                Definition = f
              };
              var frequency = LoadFrequency($"Failure '{tmp.Id}'", tmp.Element, newLogHandler);
              failure.Frequency = frequency;

              items.Add(failure!);
            }

            SafeUtils.SetPropertyValue(p, t, items);
          });

      return ret;
    }

    private static FailureFrequency LoadFrequency(string sourceId, XElement element, NewLogHandler newLogHandler)
    {
      FailureFrequency? ret = null;
      if (element.Attributes("mtbf").Any())
        try
        {
          int attval = int.Parse(element.Attribute("mtbf")!.Value);
          ret = new MtbfFailureFrequency() { MTBF = attval };
        }
        catch
        {
          newLogHandler.Invoke(
            LogLevel.WARNING,
            $"'{sourceId}' has MTBF set to {element.Attribute("mtbf")!.Value}, what cannot be represented as int. Skipped.");
        }

      if (ret == null && element.Attributes("probability").Any())
        try
        {
          int attval = int.Parse(element.Attribute("probability")!.Value);
          ret = new ProbabilityFailureFrequency() { Probability = attval };
        }
        catch
        {
          newLogHandler.Invoke(
            LogLevel.WARNING, 
            $"'{sourceId}' has probability set to {element.Attribute("probability")!.Value}, what cannot be represented as int. Skipped.");
        }
      else
      {
        newLogHandler.Invoke(LogLevel.WARNING, $"Failure id '{sourceId}' has neither MTBF nor Probability set. Default probability 5% used.");
        ret = new ProbabilityFailureFrequency() { Probability = 5 };
      }
      return ret;
    }
  }
}
