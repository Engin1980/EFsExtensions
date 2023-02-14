using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXmlLib
{
  public class EXmlException : Exception
  {
    public EXmlException(string? message) : base(message)
    {
    }

    public EXmlException(string message, Exception innerException) : base(message, innerException) { }
  }
}
