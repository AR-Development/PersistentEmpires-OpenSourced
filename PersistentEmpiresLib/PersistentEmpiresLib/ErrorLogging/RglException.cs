using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresLib.ErrorLogging
{
    public class RglException : Exception
    {
        private string oldStackTrace;
        public RglException(string message, StackTrace trace) : base(message)
        {
            this.oldStackTrace = trace.ToString();            
        }

        public override string StackTrace => this.oldStackTrace;
    }
}
