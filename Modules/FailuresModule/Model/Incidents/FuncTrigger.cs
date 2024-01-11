using System;

namespace FailuresModule.Model.Incidents
{
  public class FuncTrigger : Trigger
  {
    public Func<bool> EvaluatingFunction { get; set; }
    public string Interval { get; set; }
  }
}
