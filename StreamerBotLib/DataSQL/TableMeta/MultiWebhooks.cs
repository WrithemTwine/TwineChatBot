using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiWebhooks : IDatabaseTableMeta
    {
        public System.Int32 Id => (System.Int32)Values["Id"];
        public System.Boolean IsEnabled => (System.Boolean)Values["IsEnabled"];
        public StreamerBotLib.Enums.WebhooksSource WebhooksSource => (StreamerBotLib.Enums.WebhooksSource)Values["WebhooksSource"];
        public System.String Server => (System.String)Values["Server"];
        public StreamerBotLib.Enums.WebhooksKind Kind => (StreamerBotLib.Enums.WebhooksKind)Values["Kind"];
        public System.Boolean AddEveryone => (System.Boolean)Values["AddEveryone"];
        public System.Uri Webhook => (System.Uri)Values["Webhook"];
        public StreamerBotLib.DataSQL.DiscriminatorEnums.WebhookDataSource DataSource => (StreamerBotLib.DataSQL.DiscriminatorEnums.WebhookDataSource)Values["DataSource"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "MultiWebhooks";

        public MultiWebhooks(Models.MultiWebhooks tableData)
        {
            Values = new()
            {
                 { "Id", tableData.Id },
                 { "IsEnabled", tableData.IsEnabled },
                 { "WebhooksSource", tableData.WebhooksSource },
                 { "Server", tableData.Server },
                 { "Kind", tableData.Kind },
                 { "AddEveryone", tableData.AddEveryone },
                 { "Webhook", tableData.Webhook },
                 { "DataSource", tableData.DataSource }
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
              { "Webhook", typeof(System.Uri) },
              { "DataSource", typeof(StreamerBotLib.DataSQL.DiscriminatorEnums.WebhookDataSource) }
        };
        public object GetModelEntity()
        {
            return new Models.MultiWebhooks(

);
        }
        public void CopyUpdates(Models.MultiWebhooks modelData)
        {

        }
    }
}

