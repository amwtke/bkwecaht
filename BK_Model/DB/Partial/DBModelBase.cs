
namespace BK.Model.DB
{

    public class DBModelBase
    {
        public void CopyFrom(object obj)
        {
            if(this == obj)
                return;
            if((obj == null) || (obj.GetType() != GetType()))
                return;
            foreach(System.Reflection.PropertyInfo pi in GetType().GetProperties())
            {
                pi.SetValue(this, pi.GetValue(obj));
            }
        }

    }
}
