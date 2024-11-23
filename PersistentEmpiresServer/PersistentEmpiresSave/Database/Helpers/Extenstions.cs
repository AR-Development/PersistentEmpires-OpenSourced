using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace PersistentEmpiresSave.Database.Helpers
{
    public static class Extenstions
    {
        public static string EncodeSpecialMariaDbChars(this string tmp)
        {
            return tmp.Replace("'", @"\'");
        }
    }
}