using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.BotManagement
{
    public class BotStatusData
    {
        public BotStatusData(Platform platform)
        {
            Platform = platform;

            Elements.Add(Live);
            Elements.Add(NotLive);
            Elements.Add(Viewers);
            Elements.Add(Followers);
            Elements.Add(Category);
            Elements.Add(Title);
            Elements.Add(TokenExpiration);
        }

        public List<BotStatusDataElement> Elements { get; } = [];

        public BotStatusDataElement this[BotStatusElements element]
        {
            get
            {
                return element switch
                {
                    BotStatusElements.Live => Live,
                    BotStatusElements.NotLive => NotLive,
                    BotStatusElements.Viewers => Viewers,
                    BotStatusElements.Followers => Followers,
                    BotStatusElements.Category => Category,
                    BotStatusElements.Title => Title,
                    BotStatusElements.TokenExpiration => TokenExpiration,
                    _ => null
                };
            }
        }

        public void SetElement(object value, BotStatusElements element)
        {
            if (element == BotStatusElements.Live || element == BotStatusElements.NotLive)
            {
                if (element == BotStatusElements.Live)
                {
                    SetStreamOnline((bool)value);
                }
            }
            else
            {
                this[element].Value = value;
            }
        }
        private void SetStreamOnline(bool online = true)
        {
            Live.Value = online;
            Live.IsEnabled = online;
            Live.IsVisible = online;
            NotLive.Value = !online;
            NotLive.IsEnabled = !online;
            NotLive.IsVisible = !online;
        }

        public Platform Platform { get; }

        public BotStatusDataElement Live { get; } = new BotStatusDataElement { Element = BotStatusElements.Live, Value = false };

        public BotStatusDataElement NotLive { get; } = new BotStatusDataElement { Element = BotStatusElements.NotLive, Value = true };

        public BotStatusDataElement Viewers { get; } = new BotStatusDataElement { Element = BotStatusElements.Viewers, Value = 0 };

        public BotStatusDataElement Followers { get; } = new BotStatusDataElement { Element = BotStatusElements.Followers, Value = 0 };

        public BotStatusDataElement Category { get; } = new BotStatusDataElement { Element = BotStatusElements.Category, Value = string.Empty };

        public BotStatusDataElement Title { get; } = new BotStatusDataElement { Element = BotStatusElements.Title, Value = string.Empty };

        public BotStatusDataElement TokenExpiration { get; } = new BotStatusDataElement { Element = BotStatusElements.TokenExpiration, Value = string.Empty };
    }
}
