namespace StreamerBotLib.Interfaces
{
    public interface IDatabaseTableMeta
    {
        public string TableName { get; }

        /// <summary>
        /// key- [PropertyName]; value- [PropertyValue]
        /// </summary>
        public Dictionary<string, object> Values { get; }

        /// <summary>
        /// key- [PropertyName]; value- [PropertyType]
        /// </summary>
        public Dictionary<string, Type> Meta { get; }

        /// <summary>
        /// Gets an Database entity according to the current Meta Data and edited data
        /// </summary>
        /// <returns>A new database Models entity related to the current type.</returns>
        public object GetModelEntity();
    }
}
