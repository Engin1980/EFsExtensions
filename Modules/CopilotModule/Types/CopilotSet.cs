using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.CopilotModule.Types
{
  public class CopilotSet
  {
    public ChlaotModuleBase.ModuleUtils.MetaInfo MetaInfo { get; set; } = null!;
    public List<SpeechDefinition> SpeechDefinitions { get; set; } = null!;
  }
}
