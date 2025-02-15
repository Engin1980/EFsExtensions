using ESystem.Exceptions;
using EXmlLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  internal class RaasXmlLoader
  {
    public static Raas Load(XDocument doc)
    {

      XElement root = doc.Root!;

      Raas ret = new()
      {
        Variables = LoadRaasVariables(root),
        Speeches = LoadRaasSpeeches(root)
      };

      return ret;
    }

    private static RaasSpeeches LoadRaasSpeeches(XElement root)
    {
      XElement elm = root.LElement("speeches") ?? throw new UnexpectedNullException();
      RaasSpeeches ret = new()
      {
        TaxiToRunway = LoadRaasSpeech(elm.LElement("taxiToRunway") ?? throw new UnexpectedNullException()),
        TaxiToShortRunway = LoadRaasSpeech(elm.LElement("taxiToShortRunway") ?? throw new UnexpectedNullException()),
        OnRunway = LoadRaasSpeech(elm.LElement("onRunway") ?? throw new UnexpectedNullException()),
        OnShortRunway = LoadRaasSpeech(elm.LElement("onShortRunway") ?? throw new UnexpectedNullException()),
        LandingRunway = LoadRaasSpeech(elm.LElement("landingRunway") ?? throw new UnexpectedNullException()),
        DistanceRemaining = LoadRaasSpeech(elm.LElement("distanceRemaining") ?? throw new UnexpectedNullException())
      };
      return ret;
    }

    private static RaasSpeech LoadRaasSpeech(XElement elm)
    {
      RaasSpeech ret = new()
      {
        Speech = elm.Attribute("speech")?.Value ?? throw new UnexpectedNullException()
      };
      return ret;
    }

    private static RaasVariables LoadRaasVariables(XElement root)
    {
      XElement elm = root.LElement("variables") ?? throw new UnexpectedNullException();
      RaasVariables ret = new()
      {
        MinimalLandingDistance = LoadRaasDistanceVariable(
          nameof(RaasVariables.MinimalLandingDistance), elm.LElement("minimalLandingDistance")
          ?? throw new UnexpectedNullException()),
        MinimalTakeOffDistance = LoadRaasDistanceVariable(
          nameof(RaasVariables.MinimalTakeOffDistance), elm.LElement("minimalTakeOffDistance")
          ?? throw new UnexpectedNullException()),
        AnnouncedRemainingDistances = LoadRaasDistancesVariable(
          nameof(RaasVariables.AnnouncedRemainingDistances), elm.LElement("announcedRemainingDistances")
           ?? throw new UnexpectedNullException())
      };
      return ret;
    }

    private static RaasDistancesVariable LoadRaasDistancesVariable(string name, XElement elm)
    {
      var tmp = elm.Attribute("default")?.Value ?? throw new UnexpectedNullException();
      var dists = tmp.Split(";").Select(q => RaasDistance.Parse(q)).ToList();
      RaasDistancesVariable ret = new()
      {
        Name = name,
        Default = dists
      };
      return ret;
    }

    private static RaasDistanceVariable LoadRaasDistanceVariable(string name, XElement elm)
    {
      RaasDistanceVariable ret = new()
      {
        Name = name,
        Default = LoadRaasDistance(elm.Attribute("default") ?? throw new UnexpectedNullException())
      };
      ret.Value = ret.Default;

      return ret;
    }

    private static RaasDistance LoadRaasDistance(XAttribute xAttribute)
    {
      string val = xAttribute.Value;
      RaasDistance ret = RaasDistance.Parse(val);
      return ret;
    }
  }
}
