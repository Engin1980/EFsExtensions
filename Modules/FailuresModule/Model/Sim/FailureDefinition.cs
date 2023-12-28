using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.Sim
{
  public abstract class FailureDefinition
  {
    public FailureDefinition(string id, string title, SimConPoint simConPoint)
    {
      Id = id ?? throw new ArgumentNullException(nameof(id));
      Title = title ?? throw new ArgumentNullException(nameof(title));
      SimConPoint = simConPoint ?? throw new ArgumentNullException(nameof(simConPoint));
    }

    public string Id { get; }
    public string Title { get; }
    public SimConPoint SimConPoint { get; set; }
    public abstract string Type { get; }

    public string TypeName => GetType().Name;
  }
}
