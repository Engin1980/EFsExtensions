using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESystem.ValidityChecking
{
  public class ValidityCheckingFailedException : ApplicationException
  {
    public ValidityCheckingFailedException(string? message) : base(message)
    {
    }
  }
}
