using ChecklistModule.Types;
using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ChecklistModule
{
  public class Context : NotifyPropertyChangedBase, Eng.Chlaot.ChlaotModuleBase.IModuleProcessor
  {
    public delegate void LogDelegate(LogLevel level, string messaga);
    public event LogDelegate Log;

    public CheckSet ChecklistSet
    {
      get => base.GetProperty<CheckSet>(nameof(ChecklistSet))!;
      set => base.UpdateProperty(nameof(ChecklistSet), value);
    }

    internal void LoadFile(string xmlFile)
    {
      CheckSet tmp;
      var factory = new XmlSerializerFactory();
      XDocument doc;

      try
      {
        doc = XDocument.Load(xmlFile);
        EXml<CheckSet> exml = CreateDeserializer();
        tmp = exml.Deserialize(doc);
      }
      catch (Exception ex)
      {
        throw ex;
        // this.DoLog(LogLevel.ERROR, "Unable to read checklist-set from '{xmlFile}'.", ex);
      }

      try
      {
        BindNextChecklists(tmp);
      }
      catch (Exception ex)
      {

      }


      this.ChecklistSet = tmp;
    }

    private void BindNextChecklists(CheckSet tmp)
    {
      for (int i = 0; i < tmp.Checklists.Count; i++)
      {
        var checklist = tmp.Checklists[i];
        if (checklist.NextChecklistId is null)
        {
          if (i < tmp.Checklists.Count - 1) checklist.NextChecklist = tmp.Checklists[i + 1];
        }
        else
          checklist.NextChecklist = tmp.Checklists.Single(q=>q.Id == checklist.NextChecklistId);
      }
    }

    private EXml<CheckSet> CreateDeserializer()
    {
      EXml<CheckSet> ret = new EXml<CheckSet>();

      var oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckDefinition))
        .WithIgnoredProperty(nameof(CheckDefinition.Bytes));
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckSet))
        .WithCustomPropertyDeserialization(
          nameof(CheckSet.Checklists),
          (e, t, f, c) =>
          {
            var deser = c.ResolveElementDeserializer(typeof(List<CheckList>));
            var items = e.LElements("checklist")
              .Select(q => SafeUtils.Deserialize(q, typeof(CheckList), deser, c))
              .Cast<CheckList>()
              .ToList();
            SafeUtils.SetPropertyValue(f, t, items);
          });
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckList))
        .WithCustomPropertyDeserialization(
        nameof(CheckList.Items),
        (e, t, p, c) =>
        {
          var deser = c.ResolveElementDeserializer(typeof(List<CheckItem>));
          var items = e.LElements("item")
          .Select(q => SafeUtils.Deserialize(q, typeof(CheckItem), deser, c))
          .Cast<CheckItem>()
          .ToList();
          SafeUtils.SetPropertyValue(p, t, items);
        })
        .WithCustomPropertyDeserialization(
        nameof(CheckList.NextChecklistId),
        (e, t, p, c) =>
        {
          string? val = e.LElementOrNull("nextChecklistId")?.Attribute("id")?.Value;
          SafeUtils.SetPropertyValue(p, t, val);
        });
      ret.Context.ElementDeserializers.Insert(0, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(CheckSet))
      .WithCustomPropertyDeserialization(
        "Checklists",
        (e, t, f, c) =>
        {
          var deser = c.ResolveElementDeserializer(typeof(CheckList));
          var val = e.LElements("checklist")
          .Select(q => SafeUtils.Deserialize(q, typeof(CheckList), deser, c))
          .Cast<CheckList>()
          .ToList();
          SafeUtils.SetPropertyValue(f, t, val);
        });
      ret.Context.ElementDeserializers.Insert(0, oed);

      return ret;
    }

    private void DoLog(LogLevel level, string message, Exception? cause = null)
    {
      StringBuilder sb = new(" ");
      while (cause != null)
      {
        sb = sb.Append(cause.ToString());
        cause = cause.InnerException;
      }
      this.Log?.Invoke(level, message + sb.ToString());
    }
  }
}
