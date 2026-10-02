using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ChannelEvents : IDatabaseTableMeta
    {
        public StreamerBotLib.Models.Enums.ChannelEventActions Name { get => (StreamerBotLib.Models.Enums.ChannelEventActions)Enum.Parse(typeof(StreamerBotLib.Models.Enums.ChannelEventActions), Values["Name"]?.ToString() ?? "0"); set => Values["Name"] = value; }
        public System.Int16 RepeatMsg { get => Convert.ToInt16(Values["RepeatMsg"]); set => Values["RepeatMsg"] = value; }
        public System.Boolean AddMe { get => Convert.ToBoolean(Values["AddMe"]); set => Values["AddMe"] = value; }
        public System.Boolean Announce { get => Convert.ToBoolean(Values["Announce"]); set => Values["Announce"] = value; }
        public System.Boolean IsEnabled { get => Convert.ToBoolean(Values["IsEnabled"]); set => Values["IsEnabled"] = value; }
        public System.String Message { get => (string)Values["Message"]; set => Values["Message"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "ChannelEvents";

        public ChannelEvents(Models.ChannelEvents tableData)
        {
            Values = new()
            {
                 { "Name", tableData.Name },
                 { "RepeatMsg", tableData.RepeatMsg },
                 { "AddMe", tableData.AddMe },
                 { "Announce", tableData.Announce },
                 { "IsEnabled", tableData.IsEnabled },
                 { "Message", tableData.Message }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "Name", typeof(StreamerBotLib.Models.Enums.ChannelEventActions) },
              { "RepeatMsg", typeof(System.Int16) },
              { "AddMe", typeof(System.Boolean) },
              { "Announce", typeof(System.Boolean) },
              { "IsEnabled", typeof(System.Boolean) },
              { "Message", typeof(System.String) }
        };

        public object GetModelEntity()
        {
            return new Models.ChannelEvents(
            name: Name,
            repeatMsg: RepeatMsg,
            addMe: AddMe,
            announce: Announce,
            isEnabled: IsEnabled,
            message: Message
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
            if (modelData.Announce != Announce)
            {
                modelData.Announce = Announce;
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