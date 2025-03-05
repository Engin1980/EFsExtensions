using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents
{
    public abstract class Incident
    {
        [EXmlNonemptyString]
        public string Title { get; set; }
        public override string ToString()
        {
            return $"{Title} ({GetType().Name})";
        }
    }
}
