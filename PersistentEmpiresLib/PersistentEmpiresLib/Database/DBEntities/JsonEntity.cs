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
