using EXmlLib.Attributes;

namespace FailuresModule.Model.App
{
    public class FailId : Fail
    {
        [EXmlNonemptyString]
        public string Id { get; set; } = "";
    }
}
