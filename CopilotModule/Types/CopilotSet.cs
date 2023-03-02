using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CopilotModule.Types
{
  public class CopilotSet
  {
#pragma warning disable CS8618
    public ChlaotModuleBase.ModuleUtils.MetaInfo MetaInfo { get; set; }
    public List<SpeechDefinition> SpeechDefinitions { get; set; }
#pragma warning restore CS8618
  }
}
