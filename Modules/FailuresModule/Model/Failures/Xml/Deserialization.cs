﻿using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using EXmlLib.Deserializers;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Media.Animation;
using System.Speech.Synthesis.TtsEngine;
using System.IO.IsolatedStorage;
using ESystem.Asserting;
using static ESystem.Functions.TryCatch;
using ESystem;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Failures.Xml
{
  internal static class Deserialization
  {
    public static FailureDefinitionGroup Deserialize(string fileName)
    {
      XDocument doc = Try(
        () => XDocument.Load(fileName, LoadOptions.SetLineInfo),
        ex => throw new ApplicationException($"Unable to load xml from file '{fileName}'.", ex));
      return Deserialize(doc);
    }

    public static FailureDefinitionGroup Deserialize(XDocument doc)
    {
      return Deserialize(doc.Root!);
    }

    public static FailureDefinitionGroup Deserialize(XElement src)
    {
      var deser = CreateDeserializer();
      FailureDefinitionGroup ret = deser.Deserialize(src);
      return ret;
    }

    public static EXml<FailureDefinitionGroup> CreateDeserializer()
    {
      int index = 0;
      EXml<FailureDefinitionGroup> ret = new();

      ObjectElementDeserializer oed;

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(FailureDefinitionGroup))
        //.WithPreAction(() => Pause())
        .WithCustomPropertyDeserialization(
          nameof(FailureDefinitionGroup.Items),
          EXmlHelper.List.CreateForFlat<FailureDefinitionBase>(
             new EXmlHelper.List.DT[] {
               new("definitions", typeof(FailureDefinitionGroup)),
               new("sequence", typeof(Sequence)),
               new("set", typeof(SetFailureDefinition)),
               new("leak", typeof(LeakFailureDefinition)),
               new("sneak", typeof(SneakFailureDefinition)),
               new("stuck", typeof(StuckFailureDefinition)),
               new("toggleOnVarMismatch", typeof(ToggleOnVarMismatchFailureDefinition)),
               new("toggle", typeof(ToggleFailureDefinition))
            }))
        .WithPostAction(q => ExpandSequencies((FailureDefinitionGroup)q));
      ret.Context.ElementDeserializers.Insert(index++, oed);

      oed = new ObjectElementDeserializer()
        .WithCustomTargetType(typeof(Sequence))
        .WithPreAction(() => Pause())
        .WithCustomPropertyDeserialization(
          nameof(Sequence.Items),
          EXmlHelper.List.CreateForFlat<FailureDefinitionBase>(
             new EXmlHelper.List.DT[] {
               new("set", typeof(SetFailureDefinition)),
               new("leak", typeof(LeakFailureDefinition)),
               new("sneak", typeof(SneakFailureDefinition)),
               new("stuck", typeof(StuckFailureDefinition)),
               new("toggleOnVarMismatch", typeof(ToggleOnVarMismatchFailureDefinition)),
               new("toggle", typeof(ToggleFailureDefinition))
      }));
      ret.Context.ElementDeserializers.Insert(index++, oed);


      return ret;
    }

    private static void ExpandSequencies(FailureDefinitionGroup fdg)
    {
      for (int i = 0; i < fdg.Items.Count; i++)
      {
        FailureDefinitionBase fdb = fdg.Items[i];
        if (fdb is Sequence seq)
        {
          List<FailureDefinition> tmp = ExpandSequence(seq);
          fdg.Items.RemoveAt(i);
          foreach (FailureDefinition newItem in tmp)
          {
            fdg.Items.Insert(i++, newItem);
          }
        }
      }
    }

    private static List<FailureDefinition> ExpandSequence(Sequence seq)
    {
      List<FailureDefinition> ret = new();
      List<List<FailureDefinition>> subLists = new List<List<FailureDefinition>>();

      for (int i = seq.From; i <= seq.To; i++)
      {
        List<FailureDefinitionBase> items = seq.Items
          .Select(q => MakeClone((FailureDefinition)q))
          .Cast<FailureDefinitionBase>()
          .ToList();
        EAssert.IsTrue(items.None(q => q is FailureDefinitionGroup || q is Sequence));
        var tmp = items
          .Cast<FailureDefinition>()
          .TapEach(q => q.ExpandVariableIfExists(seq.VarRef, i))
          .ToList();
        subLists.Add(tmp);
      }

      if (subLists.Count > 0)
      {
        for (int i = 0; i < subLists[0].Count; i++)
        {
          foreach (var subList in subLists)
          {
            ret.Add(subList[i]);
          }
        }
      }

      return ret;
    }

    private static T MakeClone<T>(T obj) where T : FailureDefinition
    {
      Type objType = obj.GetType();
      var ctor = objType.GetConstructor(Array.Empty<Type>())
        ?? throw new ApplicationException($"Unable to find .ctor for type {objType}.");
      T ret = (T)ctor.Invoke(null);
      var props = objType.GetProperties();
      foreach (var prop in props)
      {
        if (prop.CanRead && prop.CanWrite)
          try
          {
            var val = prop.GetValue(obj);
            prop.SetValue(ret, val);
          }
          catch (Exception ex)
          {
            throw new ApplicationException($"Unable to transfer value of {objType}.{prop.Name}.", ex);
          }
      }
      return ret;
    }

    private static void Pause()
    {
      Console.WriteLine("x");
    }
  }
}
