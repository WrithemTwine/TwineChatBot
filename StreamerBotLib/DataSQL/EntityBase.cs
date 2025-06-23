namespace StreamerBotLib.DataSQL
{
    using StreamerBotLib.Static;

    using System.Reflection;

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

        public string GetDebugOutput()
        {
            var output = new List<string>();
            try
            {
                foreach (var propertyInfo in GetType().GetProperties())
                {
                    var value = propertyInfo.GetValue(this, null);
                    if (value != null)
                    {
                        output.Add($"{propertyInfo.Name}: {value}");
                    }
                }
                return string.Join(", ", output);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "EntityBase.GetDebugOutput");
                return string.Join(", ", output);
            }
        }
    }
}
