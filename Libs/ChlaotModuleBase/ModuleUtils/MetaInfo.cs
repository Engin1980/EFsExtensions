using EXmlLib;
using EXmlLib.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils
{
  public class MetaInfo
  {
    public static MetaInfo Deserialize(XDocument doc)
    {
      EXml<MetaInfo> d = new();
      ObjectElementDeserializer oed = new ObjectElementDeserializer().WithCustomTargetTypeAcceptancy(q => q == typeof(MetaInfo));
      d.Context.ElementDeserializers.Add(oed);

      XElement elm = doc.Root!.LElementOrNull("meta")
        ?? doc.Root!.LElementOrNull("metainfo")
        ?? doc.Root!.LElementOrNull("metaInfo")
        ?? throw new ApplicationException("Unable to find meta-info-element in document.");

      MetaInfo ret = new EXml<MetaInfo>().Deserialize(elm);
      ret.NormalizeDescription();
      return ret;
    }

    private void NormalizeDescription()
    {
      var tmp = this.Description
        .Trim()
        .Replace("\\n", "\n")
        .Replace("\\t", "\t");
      var lst = tmp.Split("\n").Select(q => q.Trim());
      this.Description = string.Join("\n", lst);
    }
    public string Version { get; set; }
    public string Label { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Web { get; set; }
    public string Email { get; set; }
    public string License { get; set; }
    public string LabelAndVersion => Label + (string.IsNullOrWhiteSpace(Version) ? "" : $" (ver.: {Version})");
  }
}
