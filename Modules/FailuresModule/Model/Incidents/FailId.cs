using EXmlLib.Attributes;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents
{
    public class FailId : Fail
    {
        [EXmlNonemptyString]
        public string Id { get; set; } = "";
    }
}
