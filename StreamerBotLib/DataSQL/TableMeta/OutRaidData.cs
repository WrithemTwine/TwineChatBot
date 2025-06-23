using StreamerBotLib.Models.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OutRaidData : IDatabaseTableMeta
    {
        public System.Int32 Id { get => (System.Int32)Values["Id"]; set => Values["Id"] = value; }
        public System.String ChannelRaided { get => (System.String)Values["ChannelRaided"]; set => Values["ChannelRaided"] = value; }
        public System.DateTime RaidDate { get => (System.DateTime)Values["RaidDate"]; set => Values["RaidDate"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "OutRaidData";

        public OutRaidData(Models.OutRaidData tableData)
        {
            Values = new()
            {
                 { "Id", tableData.Id },
                 { "ChannelRaided", tableData.ChannelRaided },
                 { "RaidDate", tableData.RaidDate }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "Id", typeof(System.Int32) },
              { "ChannelRaided", typeof(System.String) },
              { "RaidDate", typeof(System.DateTime) }
        };
        public object GetModelEntity()
        {
            return new Models.OutRaidData(
            channelRaided: ChannelRaided,
            raidDate: RaidDate
        );
        }
        public void CopyUpdates(Models.OutRaidData modelData)
        {
            if (modelData.ChannelRaided != ChannelRaided)
            {
                modelData.ChannelRaided = ChannelRaided;
            }

            if (modelData.RaidDate != RaidDate)
            {
                modelData.RaidDate = RaidDate;
            }

        }
    }
}

