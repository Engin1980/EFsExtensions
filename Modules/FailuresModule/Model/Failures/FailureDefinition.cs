using Eng.EFsExtensions.EFsExtensionsModuleBase;
using ESystem;
using ESystem.Asserting;
using EXmlLib.Attributes;
using EXmlLib.Interfaces;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Failures
{
  public abstract class FailureDefinition : FailureDefinitionBase, IXmlObjectPostDeserialize
  {
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public abstract string SimConPoint { get; }
    public abstract string Type { get; }

    public string TypeName => GetType().Name;

    public virtual void PostDeserialize()
    {
      EAssert.IsNonEmptyString(Id, $"{nameof(Id)} is empty or null");
      EAssert.IsNonEmptyString(Title, $"{nameof(Title)} is empty or null");
    }

    internal virtual void ExpandVariableIfExists(string varRef, int variableValue)
    {
      Id = ExpandVariableInString(Id, varRef, variableValue);
      Title = ExpandVariableInString(Title, varRef, variableValue);
    }

    protected static string ExpandVariableInString(string txt, string varRef, int variableValue)
    {
      string ret;
      if (txt.Contains(varRef))
      {
        ret = new StringBuilder(txt).Replace(varRef, variableValue.ToString()).ToString();
      }
      else
        ret = txt;
      return ret;
    }

    internal static void MergeFailureDefinitions(List<FailureDefinitionBase> original, FailureDefinitionGroup extending)
    {
      var originalTree = BuildExtendingTree(original);
      var extendingTree = BuildExtendingTree(extending.Items);
      foreach (var key in extendingTree.Keys)
      {
        if (originalTree.ContainsKey(key) == false) continue;

        var oldItem = originalTree[key].Where(q => q is FailureDefinition).Cast<FailureDefinition>().Single(q => q.Id == key);
        var newItem = extendingTree[key].Where(q => q is FailureDefinition).Cast<FailureDefinition>().Single(q => q.Id == key);
        originalTree[key].Remove(oldItem);
        originalTree[key].Add(newItem);
        extendingTree[key].Remove(newItem);
      }
      original.Add(extending);
      CleanUpEmptyGroups(original);
    }

    internal static void CleanUpEmptyGroups(List<FailureDefinitionBase> grp)
    {
      var fdgs = grp.Where(q => q is FailureDefinitionGroup).Cast<FailureDefinitionGroup>().ToList();
      fdgs.Where(q => q.Items.Count == 0).ToList().ForEach(q => grp.Remove(q));
      fdgs.ForEach(q => CleanUpEmptyGroups(q.Items));
    }

    private static Dictionary<string, List<FailureDefinitionBase>> BuildExtendingTree(List<FailureDefinitionBase> items)
    {
      Dictionary<string, List<FailureDefinitionBase>> ret = new();

      void processList(List<FailureDefinitionBase> lst)
      {
        foreach (var l in lst)
        {
          if (l is FailureDefinition fd)
            ret[fd.Id] = lst;
          else if (l is FailureDefinitionGroup fdg)
            processList(fdg.Items);
        }
      }

      processList(items);
      return ret;
    }

    internal static List<FailureDefinition> Flatten(List<FailureDefinitionBase> items)
    {
      var ret = items
              .FlattenRecursively((FailureDefinitionGroup q) => q.Items)
              .Cast<FailureDefinition>()
              .ToList();
      return ret;
    }
  }
}
