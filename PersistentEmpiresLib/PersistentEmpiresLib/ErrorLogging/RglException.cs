using System;
using System.Diagnostics;

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
