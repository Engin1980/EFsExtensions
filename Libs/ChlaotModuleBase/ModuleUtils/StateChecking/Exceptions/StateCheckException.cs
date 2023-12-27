using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Exceptions
{
    public class StateCheckException : ApplicationException
    {
        public StateCheckException(string? message) : base(message)
        {
        }

        public StateCheckException(string? message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
