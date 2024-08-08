using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ChannelEvents : IDatabaseTableMeta
    {
        public StreamerBotLib.Enums.ChannelEventActions Name => (StreamerBotLib.Enums.ChannelEventActions)Values["Name"];
        public System.Int16 RepeatMsg => (System.Int16)Values["RepeatMsg"];
        public System.Boolean AddMe => (System.Boolean)Values["AddMe"];
        public System.Boolean IsEnabled => (System.Boolean)Values["IsEnabled"];
        public System.String Message => (System.String)Values["Message"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "ChannelEvents";

        public ChannelEvents(Models.ChannelEvents tableData)
        {
            Values = new()
            {
                 { "Name", tableData.Name },
                 { "RepeatMsg", tableData.RepeatMsg },
                 { "AddMe", tableData.AddMe },
                 { "IsEnabled", tableData.IsEnabled },
                 { "Message", tableData.Message }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Name", typeof(StreamerBotLib.Enums.ChannelEventActions) },
              { "RepeatMsg", typeof(System.Int16) },
              { "AddMe", typeof(System.Boolean) },
              { "IsEnabled", typeof(System.Boolean) },
              { "Message", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.ChannelEvents(
                                          (StreamerBotLib.Enums.ChannelEventActions)Values["Name"],
                                          Convert.ToInt16(Values["RepeatMsg"]),
                                          (System.Boolean)Values["AddMe"],
                                          (System.Boolean)Values["IsEnabled"],
                                          (System.String)Values["Message"]
);
        }
        public void CopyUpdates(Models.ChannelEvents modelData)
        {
            if (modelData.Name != Name)
            {
                modelData.Name = Name;
            }

            if (modelData.RepeatMsg != RepeatMsg)
            {
                modelData.RepeatMsg = RepeatMsg;
            }

            if (modelData.AddMe != AddMe)
            {
                modelData.AddMe = AddMe;
            }

            if (modelData.IsEnabled != IsEnabled)
            {
                modelData.IsEnabled = IsEnabled;
            }

            if (modelData.Message != Message)
            {
                modelData.Message = Message;
            }

        }
    }
}

