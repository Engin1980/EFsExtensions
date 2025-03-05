using ChecklistTTS.Model;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChecklistTTS
{
  public class CvtStateToImage : TypedConverter<ProcessState, string>
  {
    protected override string Convert(ProcessState value, object parameter, CultureInfo culture)
    {
      string ret = value switch
      {
        ProcessState.NotProcessed => ".\\Imgs\\Ready.png",
        ProcessState.Active => ".\\Imgs\\Active.png",
        ProcessState.Processed => ".\\Imgs\\Done.png",
        ProcessState.Failed => ".\\Imgs\\Error.png",
        _ => ""
      };
      ret = System.IO.Path.Combine(
        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        ret);
      return ret;
    }

    protected override ProcessState ConvertBack(string value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
