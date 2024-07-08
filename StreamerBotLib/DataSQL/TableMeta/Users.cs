using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Users : IDatabaseTableMeta
    {
        public System.DateTime FirstDateSeen => (System.DateTime)Values["FirstDateSeen"];
        public System.DateTime CurrLoginDate => (System.DateTime)Values["CurrLoginDate"];
        public System.DateTime LastDateSeen => (System.DateTime)Values["LastDateSeen"];
        public System.String UserId => (System.String)Values["UserId"];
        public System.String UserName => (System.String)Values["UserName"];
        public StreamerBotLib.Enums.Platform Platform => (StreamerBotLib.Enums.Platform)Values["Platform"];

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Users";

        public Users(Models.Users tableData)
        {
            Values = new()
            {
                 { "FirstDateSeen", tableData.FirstDateSeen },
                 { "CurrLoginDate", tableData.CurrLoginDate },
                 { "LastDateSeen", tableData.LastDateSeen },
                 { "UserId", tableData.UserId },
                 { "UserName", tableData.UserName },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "FirstDateSeen", typeof(System.DateTime) },
              { "CurrLoginDate", typeof(System.DateTime) },
              { "LastDateSeen", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "UserName", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.Users(
                                          (System.DateTime)Values["FirstDateSeen"],
                                          (System.DateTime)Values["CurrLoginDate"],
                                          (System.DateTime)Values["LastDateSeen"],
                                          (System.String)Values["UserId"],
                                          (System.String)Values["UserName"],
                                          (StreamerBotLib.Enums.Platform)Values["Platform"]
);
        }
        public void CopyUpdates(Models.Users modelData)
        {
            if (modelData.FirstDateSeen != FirstDateSeen)
            {
                modelData.FirstDateSeen = FirstDateSeen;
            }

            if (modelData.CurrLoginDate != CurrLoginDate)
            {
                modelData.CurrLoginDate = CurrLoginDate;
            }

            if (modelData.LastDateSeen != LastDateSeen)
            {
                modelData.LastDateSeen = LastDateSeen;
            }

            if (modelData.UserId != UserId)
            {
                modelData.UserId = UserId;
            }

            if (modelData.UserName != UserName)
            {
                modelData.UserName = UserName;
            }

            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }

        }
    }
}

