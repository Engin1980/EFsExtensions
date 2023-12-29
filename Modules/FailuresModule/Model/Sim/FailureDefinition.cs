using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Sim
{
  public abstract class FailureDefinition : FailureDefinitionBase
  {
    public FailureDefinition(string id, string title, string simConPoint)
    {
      Id = id ?? throw new ArgumentNullException(nameof(id));
      Title = title ?? throw new ArgumentNullException(nameof(title));
      SimConPoint = simConPoint ?? throw new ArgumentNullException(nameof(simConPoint));
    }

    public string Id { get; private set; }
    public string Title { get; private set; }
    public string SimConPoint { get; set; }
    public abstract string Type { get; }

    public string TypeName => GetType().Name;

    internal void ExpandVariableIfExists(string varRef, int variableValue)
    {
      Id = ExpandVariable(Id, varRef, variableValue);
      Title = ExpandVariable(Title, varRef, variableValue);
      SimConPoint = ExpandVariable(SimConPoint, varRef, variableValue);
    }

    private string ExpandVariable(string txt, string varRef, int variableValue)
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
