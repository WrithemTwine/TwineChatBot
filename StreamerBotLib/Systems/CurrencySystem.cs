using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.CurrencyGames;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        private bool CurAccrualStarted;
        private bool WatchStarted;

        public bool BlackJackActive; // whether a game is active
        private bool BlackJackPlay; // bool - time to play, add no more players
        private List<LiveUser> GameBlackJackPlayers { get; set; } = [];
        private LiveUser GameCurrBlackJackPlayer = null;
        private BlackJack GameCurrBlackJack;
        private Stack<Tuple<LiveUser, string>> GameCurrBlackJackAnswer = [];
        private string GameBlackJackCurrency;

        public void StartCurrencyClock()
        {
            LogWriter.DebugLog("StartCurrencyClock", DebugLogTypes.CurrencySystem, "Starting currency clock");
            if (!CurAccrualStarted)
            {
                CurAccrualStarted = true;

                try
                {
                    ThreadManager.CreateThreadStart("StartCurrencyClock", () =>
                    {
                        Task.Run(async () =>
                        {
                            while (OptionFlags.IsStreamOnline && OptionFlags.CurrencyStart && OptionFlags.ManageUsers)
                            {
                                lock (StreamViewers)
                                {
                                    DataManage.UpdateCurrency(new(from LiveUser U in StreamViewers.GetCurrentActiveUsers()
                                                                  select U.UserId), DateTime.Now.ToLocalTime());
                                }
                                await Task.Delay(TaskDelay * (1 + (DateTime.Now.Second / 60)));
                            }
                            CurAccrualStarted = false;
                        });
                    });
                }
                catch (ThreadInterruptedException ex)
                {
                    LogWriter.LogException(ex, "StartCurrencyClock");
                }
            }
        }

        public void MonitorWatchTime()
        {
            LogWriter.DebugLog("MonitorWatchTime", DebugLogTypes.CurrencySystem, "Starting watch time monitor");
            if (!WatchStarted)
            {
                WatchStarted = true;
                try
                {
                    ThreadManager.CreateThreadStart("MonitorWatchTime", () =>
                    {
                        Task.Run(async () =>
                        {
                            // watch time accruing only works when stream is online <- i.e. watched!
                            while (OptionFlags.IsStreamOnline && OptionFlags.ManageUsers)
                            {
                                lock (StreamViewers)
                                {
                                    DataManage.UpdateWatchTime(StreamViewers.GetCurrentActiveUsers(), DateTime.Now.ToLocalTime());
                                }
                                await Task.Delay(TaskDelay * (1 + (DateTime.Now.Second / 60)));
                            }
                            WatchStarted = false;
                        });
                    });
                }
                catch (ThreadInterruptedException ex)
                {
                    LogWriter.LogException(ex, "MonitorWatchTime");
                }
            }
        }

        #region Blackjack

        public void GamePlayBlackJack(CommandData cmdrow, LiveUser CurrUser, int Wager)
        {
            if (!BlackJackPlay && cmdrow.Currency_field != "")
            {
                LogWriter.DebugLog("GamePlayBlackJack", DebugLogTypes.CurrencySystem, "Starting BlackJack game");
                if (!BlackJackActive)
                {
                    LogWriter.DebugLog("GamePlayBlackJack", DebugLogTypes.CurrencySystem, "BlackJack game not active, starting game");
                    GameBlackJackPlayers.Clear();

                    FormatResult(VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(
                        new Tuple<MsgVars, string>[]
                        {
                            new(MsgVars.user, CurrUser.UserName ),
                            new(MsgVars.housestand, OptionFlags.GameBlackJackHouseStands.ToString()),
                            new(MsgVars.hit, LocalizedMsgSystem.GetVar(MsgVars.hit)),
                            new(MsgVars.stand, LocalizedMsgSystem.GetVar(MsgVars.stand))
                        }
                        )), cmdrow.SendMsgCount, cmdrow);

                    LogWriter.DebugLog("GamePlayBlackJack", DebugLogTypes.CurrencySystem, "Creating BlackJack game");
                    GameCurrBlackJack = new();
                    ThreadManager.CreateThreadStart("GamePlayBlackJack", () => GameBlackJackStart());

                    GameBlackJackCurrency = cmdrow.Currency_field;
                    lock (GameCurrBlackJackAnswer)
                    {
                        LogWriter.DebugLog("GamePlayBlackJack", DebugLogTypes.CurrencySystem, "Clearing BlackJack answer stack");
                        GameCurrBlackJackAnswer.Clear();
                    }
                }
                GameBlackJackAddUser(CurrUser, Wager, GameBlackJackCurrency);
            }
            else if (BlackJackPlay)
            {
                LogWriter.DebugLog("GamePlayBlackJack", DebugLogTypes.CurrencySystem, "BlackJack game active, no more players allowed");
                OnProcessCommand(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackNoJoin));
            }
            else
            {
                LogWriter.DebugLog("GamePlayBlackJack", DebugLogTypes.CurrencySystem, "BlackJack game not active, no currency");
                OnProcessCommand(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackNoCurrency));
            }
        }

        private void GameBlackJackAddUser(LiveUser CurrUser, int Wager, string CurrencyName)
        {
            LogWriter.DebugLog("GameBlackJackAddUser", DebugLogTypes.CurrencySystem, "Adding user to BlackJack game");
            // check if user has enough currency
            if (!BlackJackPlay)
            {
                LogWriter.DebugLog("GameBlackJackAddUser", DebugLogTypes.CurrencySystem, "BlackJack game not active, adding user");
                if (DataManage.CheckCurrency(CurrUser, Wager, CurrencyName))
                {
                    LogWriter.DebugLog("GameBlackJackAddUser", DebugLogTypes.CurrencySystem, "User has enough currency, adding user to game");
                    GameCurrBlackJack.AddUser(CurrUser, Wager);
                    GameBlackJackPlayers.Add(CurrUser);
                    DataManage.PostCurrencyUpdate(CurrUser, -Wager, GameBlackJackCurrency);

                    OnProcessCommand(VariableParser.ParseReplace(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackPlayerJoined),
                        VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                            new(MsgVars.user,CurrUser.UserName)
                        })));
                }
                else
                {
                    LogWriter.DebugLog("GameBlackJackAddUser", DebugLogTypes.CurrencySystem, "User does not have enough currency, not adding user to game");
                    OnProcessCommand(VariableParser.ParseReplace(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackPlayerNoMoney),
                        VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                            new(MsgVars.user,CurrUser.UserName)
                        })));
                }
            }
        }

        private void GameBlackJackStart()
        {
            LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Starting BlackJack game");

            BlackJackActive = true;
            Thread.Sleep(1000 * 60 * 1); // 1000ms=1s, 1s *60s/min*1min/s = 1m time in seconds
            BlackJackPlay = true;
            GameCurrBlackJack.BuildDeck();

            if (!GameCurrBlackJack.CheckHouseWin()) // if House has 21, only way anyone else can win is with 21, all other players lose
            {
                LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "House does not have 21, starting player turns");

                OnProcessCommand(VariableParser.ParseReplace(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackStart),
                    VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.hit, LocalizedMsgSystem.GetVar(MsgVars.hit)),
                        new(MsgVars.stand, LocalizedMsgSystem.GetVar(MsgVars.stand))
                    })), false, 0);

                int WaitTime = 30000; // 30 seconds in milliseconds

                foreach (LiveUser user in GameBlackJackPlayers)
                {
                    LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Starting player turn");

                    GameCurrBlackJackPlayer = user;
                    int CurrCardCount = GameCurrBlackJack.GetUserCardValue(user);

                    if (CurrCardCount == BlackJack.BlackJackWin)
                    {
                        LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Player has 21, skipping turn");
                        OnProcessCommand($"{user.UserName}, {GameCurrBlackJack.GetUserCard(user)} {LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJack21Win)}");
                    }
                    else
                    {
                        LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Player does not have 21, starting turn");
                        while (CurrCardCount < BlackJack.BlackJackWin)
                        {
                            LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Player does not have 21, getting card");
                            OnProcessCommand($"{user.UserName}, {GameCurrBlackJack.GetUserCard(user)} " + VariableParser.ParseReplace(
                                    LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackHit),
                                    VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                    {
                                    new(MsgVars.hit, LocalizedMsgSystem.GetVar(MsgVars.hit)),
                                    new(MsgVars.stand, LocalizedMsgSystem.GetVar(MsgVars.stand))
                                    }
                                )), false, 0);
                            int ThreadWait = 0;
                            while (ThreadWait < WaitTime && GameCurrBlackJackAnswer.Count == 0)
                            {
                                Thread.Sleep(1000);
                                ThreadWait += 1000;
                            }

                            if (GameCurrBlackJackAnswer.Count == 1)
                            {
                                LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Player has responded to card request");
                                lock (GameCurrBlackJackAnswer)
                                {
                                    var Response = GameCurrBlackJackAnswer.Pop();
                                    if (Response.Item2.Contains(LocalizedMsgSystem.GetVar(MsgVars.hit)))
                                    {
                                        GameCurrBlackJack.UserWantsCard(user);
                                    }
                                    else if (Response.Item2.Contains(LocalizedMsgSystem.GetVar(MsgVars.stand)))
                                    {
                                        break;
                                    }
                                    GameCurrBlackJackAnswer.Clear();
                                }
                                CurrCardCount = GameCurrBlackJack.GetUserCardValue(user);
                            }
                            else if (ThreadWait == WaitTime || GameCurrBlackJackAnswer.Count == 0)
                            {
                                break;
                            }

                            if (CurrCardCount == BlackJack.BlackJackWin)
                            {
                                LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Player has 21, skipping turn");
                                OnProcessCommand($"{user.UserName}, {GameCurrBlackJack.GetUserCard(user)} {LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJack21Win)}", false, 0);
                            }
                            else if (CurrCardCount > BlackJack.BlackJackWin)
                            {
                                LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Player has busted, skipping turn");
                                OnProcessCommand($"{user.UserName}, {GameCurrBlackJack.GetUserCard(user)} {LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackBust)}", false, 0);
                            }
                        }
                    }
                }
            }

            LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "House turn");
            OnProcessCommand(GameCurrBlackJack.HousePlay(), false, 0);

            foreach (var U in GameCurrBlackJack.PayoutPlayers())
            {
                if (U != null)
                {
                    OnProcessCommand(U.ResultMessage);
                }
            }

            LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "Paying out players");
            DataManage.PostCurrencyUpdate(GameCurrBlackJack.PayoutPlayers(), GameBlackJackCurrency);

            GameCurrBlackJack = null;
            BlackJackPlay = false;
            BlackJackActive = false;
            LogWriter.DebugLog("GameBlackJackStart", DebugLogTypes.CurrencySystem, "BlackJack game ended");
        }

        public void GameCheckBlackJackResponse(LiveUser CurrUser, string Response)
        {
            LogWriter.DebugLog("GameCheckBlackJackResponse", DebugLogTypes.CurrencySystem, "Checking BlackJack response");
            if (CurrUser == GameCurrBlackJackPlayer)
            {
                LogWriter.DebugLog("GameCheckBlackJackResponse", DebugLogTypes.CurrencySystem, "Player has responded to BlackJack request");
                lock (GameCurrBlackJackAnswer)
                {
                    LogWriter.DebugLog("GameCheckBlackJackResponse", DebugLogTypes.CurrencySystem, "Pushing player response to stack");
                    if (GameCurrBlackJackAnswer.Count == 0)
                    {
                        GameCurrBlackJackAnswer.Push(new(CurrUser, Response.ToLower()));
                    }
                }
            }
            LogWriter.DebugLog("GameCheckBlackJackResponse", DebugLogTypes.CurrencySystem, "Player has not responded to BlackJack request");
        }

        #endregion
    }
}
