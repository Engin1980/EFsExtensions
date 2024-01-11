using EXmlLib.Attributes;

namespace FailuresModule.Model.Incidents
{
    public class FailId : Fail
    {
        [EXmlNonemptyString]
        public string Id { get; set; } = "";
    }
}
