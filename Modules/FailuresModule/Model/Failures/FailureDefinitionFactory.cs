using ESimConnect;
using ESystem;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http.Headers;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Xps.Serialization;
using System.Xml.Linq;

namespace FailuresModule.Model.Failures
{
  internal record NameTriple(string Id, string Name, string Sim);

  public class FailureDefinitionFactory
  {
    private static int ENGINES_COUNT = 4;

    public static FailureDefinitionGroup BuildFailures()
    {
      //List<FailureDefinition> ret = BuildFailuresByCode();
      FailureDefinitionGroup ret = BuildFailuresFromXml();
      return ret;
    }

    private static FailureDefinitionGroup BuildFailuresFromXml()
    {
      FailureDefinitionGroup ret;
      string fileName = ".\\Xmls\\FailureDefinitions.xml";
      EXml<FailureDefinitionGroup> exml = FailuresModule.Model.Failures.Xml.Deserialization.CreateDeserializer();
      try
      {
        XDocument doc = XDocument.Load(fileName);
        ret = exml.Deserialize(doc);
      }catch (Exception ex)
      {
        throw new ApplicationException($"Failed to deserialize failure definitions from '{fileName}'.", ex);
      }
      return ret;
    }
  }
}
