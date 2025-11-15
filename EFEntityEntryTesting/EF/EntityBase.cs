using System.Reflection;

namespace EFEntityEntryTesting.EF
{
    public class EntityBase
    {
        public object this[string name]
        {
            get
            {
                return (from PropertyInfo propertyInfo in GetType().GetProperties()
                        where propertyInfo.Name == name
                        select propertyInfo).FirstOrDefault()?.GetValue(this, null);
            }
        }
    }
}
