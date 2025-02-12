using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    internal class Users : IDatabaseTableMeta
    {
        public System.String UserName { get => (System.String)Values["UserName"]; set => Values["UserName"] = value; }
        public System.DateTime FirstDateSeen { get => (System.DateTime)Values["FirstDateSeen"]; set => Values["FirstDateSeen"] = value; }
        public System.DateTime CurrLoginDate { get => (System.DateTime)Values["CurrLoginDate"]; set => Values["CurrLoginDate"] = value; }
        public System.DateTime LastDateSeen { get => (System.DateTime)Values["LastDateSeen"]; set => Values["LastDateSeen"] = value; }
        public System.String UserId { get => (System.String)Values["UserId"]; set => Values["UserId"] = value; }
        public StreamerBotLib.Enums.Platform Platform { get => (StreamerBotLib.Enums.Platform)Values["Platform"]; set => Values["Platform"] = value; }

        public Dictionary<string, object> Values { get; }

        public string TableName { get; } = "Users";

        public Users(Models.Users tableData)
        {
            Values = new()
            {
                 { "UserName", tableData.UserName },
                 { "FirstDateSeen", tableData.FirstDateSeen },
                 { "CurrLoginDate", tableData.CurrLoginDate },
                 { "LastDateSeen", tableData.LastDateSeen },
                 { "UserId", tableData.UserId },
                 { "Platform", tableData.Platform }
            };
        }
        public Dictionary<string, Type> Meta => new()
        {
              { "UserName", typeof(System.String) },
              { "FirstDateSeen", typeof(System.DateTime) },
              { "CurrLoginDate", typeof(System.DateTime) },
              { "LastDateSeen", typeof(System.DateTime) },
              { "UserId", typeof(System.String) },
              { "Platform", typeof(StreamerBotLib.Enums.Platform) }
        };
        public object GetModelEntity()
        {
            return new Models.Users(
            userName: UserName,
            firstDateSeen: FirstDateSeen,
            currLoginDate: CurrLoginDate,
            lastDateSeen: LastDateSeen,
            userId: UserId,
            platform: Platform
        );
        }
        public void CopyUpdates(Models.Users modelData)
        {
            if (modelData.UserName != UserName)
            {
                modelData.UserName = UserName;
            }

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

            if (modelData.Platform != Platform)
            {
                modelData.Platform = Platform;
            }

        }
    }
}

