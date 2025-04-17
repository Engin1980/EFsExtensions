﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Failures
{
  public class Sequence : FailureDefinitionBase
  {
    public int From { get; set; }
    public int To { get; set; }
    public string VarRef { get; set; } = null!;
    public List<FailureDefinitionBase> Items { get; set; } = null!;
  }
}
