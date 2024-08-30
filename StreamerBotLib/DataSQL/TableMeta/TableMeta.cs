using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL.TableMeta
{
    public class TableMeta
    {
        internal IDatabaseTableMeta CurrEntity;

        private object DataEntity;

        public TableMeta SetNewEntity(Type Entity)
        {
            if (Entity == typeof(Models.BanReasons))
            {
                CurrEntity = new BanReasons(new Models.BanReasons());
            }
            else if (Entity == typeof(Models.BanRules))
            {
                CurrEntity = new BanRules(new Models.BanRules());
            }
            else if (Entity == typeof(Models.CategoryList))
            {
                CurrEntity = new CategoryList(new Models.CategoryList());
            }
            else if (Entity == typeof(Models.ChannelEvents))
            {
                CurrEntity = new ChannelEvents(new Models.ChannelEvents());
            }
            else if (Entity == typeof(Models.Clips))
            {
                CurrEntity = new Clips(new Models.Clips());
            }
            else if (Entity == typeof(Models.Commands))
            {
                CurrEntity = new Commands(new Models.Commands());
            }
            else if (Entity == typeof(Models.CommandsUser))
            {
                CurrEntity = new CommandsUser(new Models.CommandsUser());
            }
            else if (Entity == typeof(Models.Currency))
            {
                CurrEntity = new Currency(new Models.Currency());
            }
            else if (Entity == typeof(Models.CurrencyType))
            {
                CurrEntity = new CurrencyType(new Models.CurrencyType());
            }
            else if (Entity == typeof(Models.CustomWelcome))
            {
                CurrEntity = new CustomWelcome(new Models.CustomWelcome());
            }
            else if (Entity == typeof(Models.Followers))
            {
                CurrEntity = new Followers(new Models.Followers());
            }
            else if (Entity == typeof(Models.GameDeadCounter))
            {
                CurrEntity = new GameDeadCounter(new Models.GameDeadCounter());
            }
            else if (Entity == typeof(Models.GiveawayUserData))
            {
                CurrEntity = new GiveawayUserData(new Models.GiveawayUserData());
            }
            else if (Entity == typeof(Models.InRaidData))
            {
                CurrEntity = new InRaidData(new Models.InRaidData());
            }
            else if (Entity == typeof(Models.LearnMsgs))
            {
                CurrEntity = new LearnMsgs(new Models.LearnMsgs());
            }
            else if (Entity == typeof(Models.ModeratorApprove))
            {
                CurrEntity = new ModeratorApprove(new Models.ModeratorApprove());
            }
            else if (Entity == typeof(Models.MultiChannels))
            {
                CurrEntity = new MultiChannels(new Models.MultiChannels());
            }
            else if (Entity == typeof(Models.MultiLiveStreams))
            {
                CurrEntity = new MultiLiveStreams(new Models.MultiLiveStreams());
            }
            else if (Entity == typeof(Models.MultiMsgEndPoints))
            {
                CurrEntity = new MultiMsgEndPoints(new Models.MultiMsgEndPoints());
            }
            else if (Entity == typeof(Models.MultiSummaryLiveStreams))
            {
                CurrEntity = new MultiSummaryLiveStreams(new Models.MultiSummaryLiveStreams());
            }
            else if (Entity == typeof(Models.OutRaidData))
            {
                CurrEntity = new OutRaidData(new Models.OutRaidData());
            }
            else if (Entity == typeof(Models.OverlayServices))
            {
                CurrEntity = new OverlayServices(new Models.OverlayServices());
            }
            else if (Entity == typeof(Models.OverlayTicker))
            {
                CurrEntity = new OverlayTicker(new Models.OverlayTicker());
            }
            else if (Entity == typeof(Models.Quotes))
            {
                CurrEntity = new Quotes(new Models.Quotes());
            }
            else if (Entity == typeof(Models.ShoutOuts))
            {
                CurrEntity = new ShoutOuts(new Models.ShoutOuts());
            }
            else if (Entity == typeof(Models.StreamStats))
            {
                CurrEntity = new StreamStats(new Models.StreamStats());
            }
            else if (Entity == typeof(Models.Users))
            {
                CurrEntity = new Users(new Models.Users());
            }
            else if (Entity == typeof(Models.UserStats))
            {
                CurrEntity = new UserStats(new Models.UserStats());
            }
            else if (Entity == typeof(Models.Webhooks))
            {
                CurrEntity = new Webhooks(new Models.Webhooks());
            }
            
            return this;
        }

        public TableMeta SetExistingEntity(object Entity)
        {
            DataEntity = Entity;

            if (Entity.GetType() == typeof(Models.BanReasons))
            {
                CurrEntity = new BanReasons((Models.BanReasons)Entity);
            }
            else if (Entity.GetType() == typeof(Models.BanRules))
            {
                CurrEntity = new BanRules((Models.BanRules)Entity);
            }
            else if (Entity.GetType() == typeof(Models.CategoryList))
            {
                CurrEntity = new CategoryList((Models.CategoryList)Entity);
            }
            else if (Entity.GetType() == typeof(Models.ChannelEvents))
            {
                CurrEntity = new ChannelEvents((Models.ChannelEvents)Entity);
            }
            else if (Entity.GetType() == typeof(Models.Clips))
            {
                CurrEntity = new Clips((Models.Clips)Entity);
            }
            else if (Entity.GetType() == typeof(Models.Commands))
            {
                CurrEntity = new Commands((Models.Commands)Entity);
            }
            else if (Entity.GetType() == typeof(Models.CommandsUser))
            {
                CurrEntity = new CommandsUser((Models.CommandsUser)Entity);
            }
            else if (Entity.GetType() == typeof(Models.Currency))
            {
                CurrEntity = new Currency((Models.Currency)Entity);
            }
            else if (Entity.GetType() == typeof(Models.CurrencyType))
            {
                CurrEntity = new CurrencyType((Models.CurrencyType)Entity);
            }
            else if (Entity.GetType() == typeof(Models.CustomWelcome))
            {
                CurrEntity = new CustomWelcome((Models.CustomWelcome)Entity);
            }
            else if (Entity.GetType() == typeof(Models.Followers))
            {
                CurrEntity = new Followers((Models.Followers)Entity);
            }
            else if (Entity.GetType() == typeof(Models.GameDeadCounter))
            {
                CurrEntity = new GameDeadCounter((Models.GameDeadCounter)Entity);
            }
            else if (Entity.GetType() == typeof(Models.GiveawayUserData))
            {
                CurrEntity = new GiveawayUserData((Models.GiveawayUserData)Entity);
            }
            else if (Entity.GetType() == typeof(Models.InRaidData))
            {
                CurrEntity = new InRaidData((Models.InRaidData)Entity);
            }
            else if (Entity.GetType() == typeof(Models.LearnMsgs))
            {
                CurrEntity = new LearnMsgs((Models.LearnMsgs)Entity);
            }
            else if (Entity.GetType() == typeof(Models.ModeratorApprove))
            {
                CurrEntity = new ModeratorApprove((Models.ModeratorApprove)Entity);
            }
            else if (Entity.GetType() == typeof(Models.MultiChannels))
            {
                CurrEntity = new MultiChannels((Models.MultiChannels)Entity);
            }
            else if (Entity.GetType() == typeof(Models.MultiLiveStreams))
            {
                CurrEntity = new MultiLiveStreams((Models.MultiLiveStreams)Entity);
            }
            else if (Entity.GetType() == typeof(Models.MultiMsgEndPoints))
            {
                CurrEntity = new MultiMsgEndPoints((Models.MultiMsgEndPoints)Entity);
            }
            else if (Entity.GetType() == typeof(Models.MultiSummaryLiveStreams))
            {
                CurrEntity = new MultiSummaryLiveStreams((Models.MultiSummaryLiveStreams)Entity);
            }
            else if (Entity.GetType() == typeof(Models.OutRaidData))
            {
                CurrEntity = new OutRaidData((Models.OutRaidData)Entity);
            }
            else if (Entity.GetType() == typeof(Models.OverlayServices))
            {
                CurrEntity = new OverlayServices((Models.OverlayServices)Entity);
            }
            else if (Entity.GetType() == typeof(Models.OverlayTicker))
            {
                CurrEntity = new OverlayTicker((Models.OverlayTicker)Entity);
            }
            else if (Entity.GetType() == typeof(Models.Quotes))
            {
                CurrEntity = new Quotes((Models.Quotes)Entity);
            }
            else if (Entity.GetType() == typeof(Models.ShoutOuts))
            {
                CurrEntity = new ShoutOuts((Models.ShoutOuts)Entity);
            }
            else if (Entity.GetType() == typeof(Models.StreamStats))
            {
                CurrEntity = new StreamStats((Models.StreamStats)Entity);
            }
            else if (Entity.GetType() == typeof(Models.Users))
            {
                CurrEntity = new Users((Models.Users)Entity);
            }
            else if (Entity.GetType() == typeof(Models.UserStats))
            {
                CurrEntity = new UserStats((Models.UserStats)Entity);
            }
            else if (Entity.GetType() == typeof(Models.Webhooks))
            {
                CurrEntity = new Webhooks((Models.Webhooks)Entity);
            }
            
            return this;
        }

        public object GetUpdatedEntity(IDatabaseTableMeta Update)
        {
            if (DataEntity.GetType() == typeof(Models.BanReasons))
            {
                ((BanReasons)Update).CopyUpdates((Models.BanReasons)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.BanRules))
            {
                ((BanRules)Update).CopyUpdates((Models.BanRules)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.CategoryList))
            {
                ((CategoryList)Update).CopyUpdates((Models.CategoryList)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.ChannelEvents))
            {
                ((ChannelEvents)Update).CopyUpdates((Models.ChannelEvents)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.Clips))
            {
                ((Clips)Update).CopyUpdates((Models.Clips)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.Commands))
            {
                ((Commands)Update).CopyUpdates((Models.Commands)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.CommandsUser))
            {
                ((CommandsUser)Update).CopyUpdates((Models.CommandsUser)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.Currency))
            {
                ((Currency)Update).CopyUpdates((Models.Currency)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.CurrencyType))
            {
                ((CurrencyType)Update).CopyUpdates((Models.CurrencyType)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.CustomWelcome))
            {
                ((CustomWelcome)Update).CopyUpdates((Models.CustomWelcome)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.Followers))
            {
                ((Followers)Update).CopyUpdates((Models.Followers)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.GameDeadCounter))
            {
                ((GameDeadCounter)Update).CopyUpdates((Models.GameDeadCounter)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.GiveawayUserData))
            {
                ((GiveawayUserData)Update).CopyUpdates((Models.GiveawayUserData)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.InRaidData))
            {
                ((InRaidData)Update).CopyUpdates((Models.InRaidData)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.LearnMsgs))
            {
                ((LearnMsgs)Update).CopyUpdates((Models.LearnMsgs)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.ModeratorApprove))
            {
                ((ModeratorApprove)Update).CopyUpdates((Models.ModeratorApprove)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.MultiChannels))
            {
                ((MultiChannels)Update).CopyUpdates((Models.MultiChannels)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.MultiLiveStreams))
            {
                ((MultiLiveStreams)Update).CopyUpdates((Models.MultiLiveStreams)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.MultiMsgEndPoints))
            {
                ((MultiMsgEndPoints)Update).CopyUpdates((Models.MultiMsgEndPoints)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.MultiSummaryLiveStreams))
            {
                ((MultiSummaryLiveStreams)Update).CopyUpdates((Models.MultiSummaryLiveStreams)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.OutRaidData))
            {
                ((OutRaidData)Update).CopyUpdates((Models.OutRaidData)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.OverlayServices))
            {
                ((OverlayServices)Update).CopyUpdates((Models.OverlayServices)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.OverlayTicker))
            {
                ((OverlayTicker)Update).CopyUpdates((Models.OverlayTicker)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.Quotes))
            {
                ((Quotes)Update).CopyUpdates((Models.Quotes)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.ShoutOuts))
            {
                ((ShoutOuts)Update).CopyUpdates((Models.ShoutOuts)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.StreamStats))
            {
                ((StreamStats)Update).CopyUpdates((Models.StreamStats)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.Users))
            {
                ((Users)Update).CopyUpdates((Models.Users)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.UserStats))
            {
                ((UserStats)Update).CopyUpdates((Models.UserStats)DataEntity);
                return DataEntity;
            }
            else if (DataEntity.GetType() == typeof(Models.Webhooks))
            {
                ((Webhooks)Update).CopyUpdates((Models.Webhooks)DataEntity);
                return DataEntity;
            }
            else return null;
        }
    }
}

