using System;

namespace FailuresModule.Model.App
{
  public class FuncTrigger : Trigger
  {
    public Func<bool> EvaluatingFunction { get; set; }
    public string Interval { get; set; }
  }
}
