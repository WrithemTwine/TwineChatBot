using StreamerBotLib.Models.BotManagement;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        public ObservableCollection<BotStatusData> BotStatusData { get; }
        public BotStatusData BotStatusSummary = new(Platform.Default);

        public void AddBotStatusItem(Platform platform)
        {
            BotStatusData.UniqueAdd(new(platform), (b) => b.Platform == platform);
        }

        public void UpdateBotStatusData(Platform platform, BotStatusElements element, object value)
        {
            BotStatusData.Where(b => b.Platform == platform).Select(b => b).FirstOrDefault()?.SetElement(value, element);

            switch (element)
            {
                case BotStatusElements.Live:
                case BotStatusElements.NotLive:
                    BotStatusSummary.SetElement( BotStatusData.Any(b => (bool)b.Live.Value == true),
                        BotStatusElements.Live);
                    break;
                case BotStatusElements.Followers:
                    BotStatusSummary.Followers.Value = BotStatusData.Sum(b => (int)b.Followers.Value);
                    break;
                case BotStatusElements.Viewers:
                    BotStatusSummary.Viewers.Value = BotStatusData.Sum(b => (int)b.Viewers.Value);
                    break;
                case BotStatusElements.Category:
                    if (BotStatusData.Any(a => (bool)a.Live.Value == true))
                    {
                        BotStatusSummary.Category.Value = BotStatusData.Where(b => (bool)b.Live.Value == true).Select(b => b.Category.Value).FirstOrDefault();
                    }
                    else
                    {
                        BotStatusSummary.Category.Value = BotStatusData.Where(b => !string.IsNullOrEmpty((string)b.Category.Value)).Select(b => b.Category.Value).FirstOrDefault();
                    }
                    break;
                case BotStatusElements.Title:
                    if (BotStatusData.Any(a => (bool)a.Live.Value == true))
                    {
                        BotStatusSummary.Title.Value = BotStatusData.Where(b => (bool)b.Live.Value == true).Select(b => b.Title.Value).FirstOrDefault();
                    }
                    else
                    {
                        BotStatusSummary.Title.Value = BotStatusData.Where(b => !string.IsNullOrEmpty((string)b.Title.Value)).Select(b => b.Title.Value).FirstOrDefault();
                    }
                    break;
                case BotStatusElements.TokenExpiration:
                    var times = BotStatusData
                                .Select(b => b.TokenExpiration.Value)
                                .Where(v => v is DateTime)
                                .Cast<DateTime>()
                                .ToList();

                    if (times.Count > 0)
                        BotStatusSummary.TokenExpiration.Value = times.Min();
                    else
                    {
                        var s = BotStatusData
                            .Select(b => b.TokenExpiration.Value as string)
                            .FirstOrDefault(v => !string.IsNullOrEmpty(v));
                        BotStatusSummary.TokenExpiration.Value = s ?? string.Empty;
                    }
                    break;
            }
        }

        public void RemoveBotStatusItem(Platform platform)
        {
            var item = BotStatusData.Where(b => b.Platform == platform).Select(b => b).FirstOrDefault();
            if (item != null)
            {
                BotStatusData.Remove(item);
            }
        }

    }
}
