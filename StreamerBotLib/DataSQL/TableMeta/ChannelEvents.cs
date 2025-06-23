using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class ChannelEvents : IDatabaseTableMeta
    {
        public ChannelEventActions Name { get => (ChannelEventActions)Values["Name"]; set => Values["Name"] = value; }
        public System.Int16 RepeatMsg { get => (System.Int16)Values["RepeatMsg"]; set => Values["RepeatMsg"] = value; }
        public System.Boolean AddMe { get => (System.Boolean)Values["AddMe"]; set => Values["AddMe"] = value; }
        public System.Boolean IsEnabled { get => (System.Boolean)Values["IsEnabled"]; set => Values["IsEnabled"] = value; }
        public System.String Message { get => (System.String)Values["Message"]; set => Values["Message"] = value; }

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
              { "Name", typeof(ChannelEventActions) },
              { "RepeatMsg", typeof(System.Int16) },
              { "AddMe", typeof(System.Boolean) },
              { "IsEnabled", typeof(System.Boolean) },
              { "Message", typeof(System.String) }
        };
        public object GetModelEntity()
        {
            return new Models.ChannelEvents(
            name: Name,
            repeatMsg: Convert.ToInt16(RepeatMsg),
            addMe: AddMe,
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

