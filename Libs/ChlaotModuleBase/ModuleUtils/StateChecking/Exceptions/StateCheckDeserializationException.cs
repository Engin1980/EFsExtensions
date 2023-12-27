using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Exceptions
{
    public class StateCheckDeserializationException : ApplicationException
    {
        public StateCheckDeserializationException(string? message) : base(message)
        {
        }

        public StateCheckDeserializationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
