using StreamerBotLib.Enums;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay.Enums;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class MultiWebhooks : IDatabaseTableMeta
    {
        public System.Int32 Id { get => (System.Int32)Values["Id"]; set => Values["Id"] = value; }
        public System.Boolean IsEnabled { get => (System.Boolean)Values["IsEnabled"]; set => Values["IsEnabled"] = value; }
        public StreamerBotLib.Enums.WebhooksSource WebhooksSource { get => (StreamerBotLib.Enums.WebhooksSource)Values["WebhooksSource"]; set => Values["WebhooksSource"] = value; }
        public System.String Server { get => (System.String)Values["Server"]; set => Values["Server"] = value; }
        public StreamerBotLib.Enums.WebhooksKind Kind { get => (StreamerBotLib.Enums.WebhooksKind)Values["Kind"]; set => Values["Kind"] = value; }
        public System.Boolean AddEveryone { get => (System.Boolean)Values["AddEveryone"]; set => Values["AddEveryone"] = value; }
        public System.Uri Webhook { get => (System.Uri)Values["Webhook"]; set => Values["Webhook"] = value; }

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
            return new Models.MultiWebhooks(
            isEnabled: IsEnabled, 
            webhooksSource: WebhooksSource, 
            server: Server, 
            kind: Kind, 
            addEveryone: AddEveryone, 
            webhook: Webhook
        );
        }
        public void CopyUpdates(Models.MultiWebhooks modelData)
        {
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

