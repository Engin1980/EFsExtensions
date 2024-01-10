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
        ?? throw new ApplicationException("Unable to find meta-info-element in document.");

      MetaInfo ret = new EXml<MetaInfo>().Deserialize(elm);
      return ret;
    }
    public string Label { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Web { get; set; }
    public string Email { get; set; }
    public string License { get; set; }
  }
}
