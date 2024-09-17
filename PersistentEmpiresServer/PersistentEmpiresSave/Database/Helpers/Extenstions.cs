using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresSave.Database.Helpers
{
    internal static class Extenstions
    {
        internal static string EncodeSpecialMariaDbChars(this string tmp)
        {
            return tmp.Replace("'", @"\'");
        }
    }
}
