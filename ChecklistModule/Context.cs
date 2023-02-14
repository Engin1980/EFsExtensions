using ChecklistModule.Deserializers;
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
      CheckSet? tmp;
      var factory = new XmlSerializerFactory();
      XDocument doc;

      try
      {
        doc = XDocument.Load(xmlFile);
        EXml<CheckSet> exml = CreateDeserializer();
        this.ChecklistSet = exml.Deserialize(doc);
      }
      catch (Exception ex)
      {
        throw ex;
        // this.DoLog(LogLevel.ERROR, "Unable to read checklist-set from '{xmlFile}'.", ex);
      }
    }

    private EXml<CheckSet> CreateDeserializer()
    {
      EXml<CheckSet> ret = new EXml<CheckSet>();

      var oed = new ObjectElementDeserializer(
          t => t == typeof(CheckDefinition),
          new Dictionary<string, ObjectElementDeserializer.PropertyDeserializeHandler>
        {
          { "Bytes", null }
        });
      ret.Context.ElementDeserializers.Insert(0, oed);


      oed = new ObjectElementDeserializer(
          q => q == typeof(CheckSet),
          new Dictionary<string, ObjectElementDeserializer.PropertyDeserializeHandler>
          {
            { "Checklists", (e,t,f,c) => {
              var deser = c.ResolveElementDeserializer(typeof(List<CheckList>));
              var items = e.LElements("checklist")
                .Select(q=>SafeUtils.Deserialize(q, typeof(CheckList), deser, c))
                .ToList();
              SafeUtils.SetPropertyValue(f, t, items);
            }}
        });
      ret.Context.ElementDeserializers.Insert(0, oed);

      ret.Context.ElementDeserializers.Insert(0, new CheckSetDeserializer());
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
