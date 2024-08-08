using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class OutRaidData : IDatabaseTableMeta
    {
        public System.Int32 Id => (System.Int32)Values["Id"];
        public System.String ChannelRaided => (System.String)Values["ChannelRaided"];
        public System.DateTime RaidDate => (System.DateTime)Values["RaidDate"];

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
                                          Convert.ToInt32(Values["Id"]),
                                          (System.String)Values["ChannelRaided"],
                                          (System.DateTime)Values["RaidDate"]
);
        }
        public void CopyUpdates(Models.OutRaidData modelData)
        {
            if (modelData.Id != Id)
            {
                modelData.Id = Id;
            }

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

