using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class Json<T>
    {
        public Json(T Value)
        {
            this.Value = Value;
        }
        public T Value;
    }
}
