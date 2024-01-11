using ESystem.Asserting;
using EXmlLib.Attributes;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace FailuresModule.Model.Failures
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
      EAssert.IsNonEmptyString(Id);
      EAssert.IsNonEmptyString(Title);
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
  }
}
