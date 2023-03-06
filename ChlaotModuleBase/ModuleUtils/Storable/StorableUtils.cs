using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.Storable
{
  public static  class StorableUtils
  {
    public static CommonFileDialogFilter CreateCommonFileDialogFilter(string title, string extension)
    {
      CommonFileDialogFilter cfdf = new(title, extension);
      Type t = cfdf.GetType();
      var fieldInfo = t.GetField("_extensions",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        ?? throw new ApplicationException("Field with name '_extensions' not found; reflection error.");
      System.Collections.ObjectModel.Collection<string?> col =
        (System.Collections.ObjectModel.Collection<string?>)fieldInfo.GetValue(cfdf)!;
      col.Clear();
      col.Add(extension);
      return cfdf;
    }
  }
}
