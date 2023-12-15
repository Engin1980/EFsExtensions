using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Model.App
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
