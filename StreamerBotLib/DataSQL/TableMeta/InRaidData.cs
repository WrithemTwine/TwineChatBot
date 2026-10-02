using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class InRaidData : IDatabaseTableMeta
    {
        public System.Int32 ViewerCount { get => Convert.ToInt32(Values["ViewerCount"]); set => Values["ViewerCount"] = value; }
        public System.DateTime RaidDate { get => Convert.ToDateTime(Values["RaidDate"]); set => Values["RaidDate"] = value; }
        public System.String Category { get => (string)Values["Category"]; set => Values["Category"] = value; }
        public System.String UserId { get => (string)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Models.Enums.Platform Platform { get => (StreamerBotLib.Models.Enums.Platform)Enum.Parse(typeof(StreamerBotLib.Models.Enums.Platform), Values["Platform"]?.ToString() ?? "0"); set => Values["Platform"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "InRaidData";

        public InRaidData(Models.InRaidData tableData)
        {
            Values = new()
            {
                 { "ViewerCount", tableData.ViewerCount },
                 { "RaidDate", tableData.RaidDate },
                 { "Category", tableData.Category },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }

        public Dictionary<string, Type> Meta => new()
        {
              { "ViewerCount", typeof(System.Int32) },
              { "RaidDate", typeof(System.DateTime) },
              { "Category", typeof(System.String) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Models.Enums.Platform) }
        };

        public object GetModelEntity()
        {
            return new Models.InRaidData(
            viewerCount: ViewerCount,
            raidDate: RaidDate,
            category: Category,
            userId: UserId,
            platform: Platform
            );
        }

        public void CopyUpdates(Models.InRaidData modelData)
        {
            if (modelData.ViewerCount != ViewerCount)
            {
                modelData.ViewerCount = ViewerCount;
            }
            if (modelData.RaidDate != RaidDate)
            {
                modelData.RaidDate = RaidDate;
            }
            if (modelData.Category != Category)
            {
                modelData.Category = Category;
            }
            if (modelData.UserId != UserId)
            {
                modelData.UserId = UserId;
            }
            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }
        }
    }
}