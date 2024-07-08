using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiMsgEndPoints : IDatabaseTableMeta
    {
        public System.Int32 Id => (System.Int32)Values["Id"];
        public System.Boolean IsEnabled => (System.Boolean)Values["IsEnabled"];
        public StreamerBotLib.Enums.WebhooksSource WebhooksSource => (StreamerBotLib.Enums.WebhooksSource)Values["WebhooksSource"];
        public System.String Server => (System.String)Values["Server"];
        public StreamerBotLib.Enums.WebhooksKind Kind => (StreamerBotLib.Enums.WebhooksKind)Values["Kind"];
        public System.Boolean AddEveryone => (System.Boolean)Values["AddEveryone"];
        public System.Uri Webhook => (System.Uri)Values["Webhook"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "MultiMsgEndPoints";

        public MultiMsgEndPoints(Models.MultiMsgEndPoints tableData)
        {
            Values = new()
            {
                 { "Id", tableData.Id },
                 { "IsEnabled", tableData.IsEnabled },
                 { "WebhooksSource", tableData.WebhooksSource },
                 { "Server", tableData.Server },
                 { "Kind", tableData.Kind },
                 { "AddEveryone", tableData.AddEveryone },
                 { "Webhook", tableData.Webhook }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Id", typeof(System.Int32) },
              { "IsEnabled", typeof(System.Boolean) },
              { "WebhooksSource", typeof(StreamerBotLib.Enums.WebhooksSource) },
              { "Server", typeof(System.String) },
              { "Kind", typeof(StreamerBotLib.Enums.WebhooksKind) },
              { "AddEveryone", typeof(System.Boolean) },
              { "Webhook", typeof(System.Uri) }
        };
        public object GetModelEntity()
        {
            return new Models.MultiMsgEndPoints(
                                          (System.Int32)Values["Id"],
                                          (System.Boolean)Values["IsEnabled"],
                                          (StreamerBotLib.Enums.WebhooksSource)Values["WebhooksSource"],
                                          (System.String)Values["Server"],
                                          (StreamerBotLib.Enums.WebhooksKind)Values["Kind"],
                                          (System.Boolean)Values["AddEveryone"],
                                          (System.Uri)Values["Webhook"]
);
        }
        public void CopyUpdates(Models.MultiMsgEndPoints modelData)
        {
            if (modelData.Id != Id)
            {
                modelData.Id = Id;
            }

            if (modelData.IsEnabled != IsEnabled)
            {
                modelData.IsEnabled = IsEnabled;
            }

            if (modelData.WebhooksSource != WebhooksSource)
            {
                modelData.WebhooksSource = WebhooksSource;
            }

            if (modelData.Server != Server)
            {
                modelData.Server = Server;
            }

            if (modelData.Kind != Kind)
            {
                modelData.Kind = Kind;
            }

            if (modelData.AddEveryone != AddEveryone)
            {
                modelData.AddEveryone = AddEveryone;
            }

            if (modelData.Webhook != Webhook)
            {
                modelData.Webhook = Webhook;
            }

        }
    }
}

