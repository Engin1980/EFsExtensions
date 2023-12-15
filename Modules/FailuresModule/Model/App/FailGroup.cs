using System.Collections.Generic;

namespace FailuresModule.Model.App
{
    public class FailGroup : Fail
    {
        public enum ESelection
        {
            None, One, All
        }

        public ESelection Selection { get; set; } = ESelection.One;
        public List<Fail> Items { get; set; } = new List<Fail>();
    }
}
