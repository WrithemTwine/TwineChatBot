using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.CurrencyGames;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace StreamerBotLib.Systems
{
    internal partial class ActionSystem
    {
        private bool CurAccrualStarted;
        private bool WatchStarted;

        public bool BlackJackActive; // whether a game is active
        private bool BlackJackPlay; // bool - time to play, add no more players
        private List<LiveUser> GameBlackJackPlayers { get; set; } = new();
        private LiveUser GameCurrBlackJackPlayer = null;
        private BlackJack GameCurrBlackJack;
        private Stack<Tuple<LiveUser, string>> GameCurrBlackJackAnswer = new();
        private string GameBlackJackCurrency;


        public void StartCurrencyClock()
        {
            if (!CurAccrualStarted)
            {
                CurAccrualStarted = true;

                try
                {
                    ThreadManager.CreateThreadStart(() =>
                    {
                        while (OptionFlags.IsStreamOnline && OptionFlags.CurrencyStart && OptionFlags.ManageUsers)
                        {
                            lock (CurrUsers)
                            {
                                DataManage.UpdateCurrency(new(from LiveUser U in CurrUsers
                                                              select U.UserName), DateTime.Now.ToLocalTime());
                            }
                            // randomly extend the time delay up to 2times as long
                            Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                        }
                        CurAccrualStarted = false;
                    });
                }
                catch (ThreadInterruptedException ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }
            }
        }

        public void MonitorWatchTime()
        {
            if (!WatchStarted)
            {
                WatchStarted = true;
                try
                {
                    ThreadManager.CreateThreadStart(() =>
                    {
                        // watch time accruing only works when stream is online <- i.e. watched!
                        while (OptionFlags.IsStreamOnline && OptionFlags.ManageUsers)
                        {
                            lock (CurrUsers)
                            {
                                DataManage.UpdateWatchTime(new List<string>(from LiveUser U in CurrUsers
                                                                            select U.UserName), DateTime.Now.ToLocalTime());
                            }
                            // randomly extend the time delay up to 2times as long
                            Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                        }
                        WatchStarted = false;
                    });
                }
                catch (ThreadInterruptedException ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }
            }
        }

        #region Blackjack

        public void GamePlayBlackJack(CommandData cmdrow, LiveUser CurrUser, int Wager)
        {
            if (!BlackJackPlay && cmdrow.Currency_field != "")
            {
                if (!BlackJackActive)
                {
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

                    ThreadManager.CreateThreadStart(() => GameBlackJackStart());
                    GameCurrBlackJack = new();

                    GameBlackJackCurrency = cmdrow.Currency_field;
                    lock (GameCurrBlackJackAnswer)
                    {
                        GameCurrBlackJackAnswer.Clear();
                    }
                }
                GameBlackJackAddUser(CurrUser, Wager, GameBlackJackCurrency);
            }
            else if (BlackJackPlay)
            {
                OnProcessCommand(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackNoJoin));
            }
            else
            {
                OnProcessCommand(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackNoCurrency));
            }
        }

        private void GameBlackJackAddUser(LiveUser CurrUser, int Wager, string CurrencyName)
        {
            // check if user has enough currency
            if (!BlackJackPlay)
            {
                if (DataManage.CheckCurrency(CurrUser, Wager, CurrencyName))
                {
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
            BlackJackActive = true;
            Thread.Sleep(1000 * 60 * 1); // 1000ms=1s, 1s *60s/min*1min/s = 1m time in seconds
            BlackJackPlay = true;
            GameCurrBlackJack.BuildDeck();

            if (!GameCurrBlackJack.CheckHouseWin()) // if House has 21, only way anyone else can win is with 21, all other players lose
            {
                OnProcessCommand(VariableParser.ParseReplace(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackStart),
                    VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.hit, LocalizedMsgSystem.GetVar(MsgVars.hit)),
                        new(MsgVars.stand, LocalizedMsgSystem.GetVar(MsgVars.stand))
                    })));

                int WaitTime = 30000; // 30 seconds in milliseconds

                foreach (LiveUser user in GameBlackJackPlayers)
                {
                    GameCurrBlackJackPlayer = user;
                    int CurrCardCount = GameCurrBlackJack.GetUserCardValue(user);

                    if (CurrCardCount == BlackJack.BlackJackWin)
                    {
                        OnProcessCommand($"{user.UserName}, {GameCurrBlackJack.GetUserCard(user)} {LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJack21Win)}");
                    }
                    else
                    {
                        while (CurrCardCount < BlackJack.BlackJackWin)
                        {
                            OnProcessCommand($"{user.UserName}, {GameCurrBlackJack.GetUserCard(user)} " + VariableParser.ParseReplace(
                                    LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackHit),
                                    VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                    {
                                    new(MsgVars.hit, LocalizedMsgSystem.GetVar(MsgVars.hit)),
                                    new(MsgVars.stand, LocalizedMsgSystem.GetVar(MsgVars.stand))
                                    }
                                )));
                            int ThreadWait = 0;
                            while (ThreadWait < WaitTime && GameCurrBlackJackAnswer.Count == 0)
                            {
                                Thread.Sleep(1000);
                                ThreadWait += 1000;
                            }

                            if (GameCurrBlackJackAnswer.Count == 1)
                            {
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
                                OnProcessCommand($"{user.UserName}, {GameCurrBlackJack.GetUserCard(user)} {LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJack21Win)}");
                            }
                            else if (CurrCardCount > BlackJack.BlackJackWin)
                            {
                                OnProcessCommand($"{user.UserName}, {GameCurrBlackJack.GetUserCard(user)} {LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackBust)}");
                            }
                        }
                    }
                }
            }

            OnProcessCommand(GameCurrBlackJack.HousePlay(), 0);

            foreach (var U in GameCurrBlackJack.PayoutPlayers())
            {
                if (U != null)
                {
                    OnProcessCommand(U.ResultMessage);
                    DataManage.PostCurrencyUpdate(U.Player, U.Payout, GameBlackJackCurrency);
                }
            }

            GameCurrBlackJack = null;
            BlackJackPlay = false;
            BlackJackActive = false;
        }

        public void GameCheckBlackJackResponse(LiveUser CurrUser, string Response)
        {
            if (CurrUser == GameCurrBlackJackPlayer)
            {
                lock (GameCurrBlackJackAnswer)
                {
                    if (GameCurrBlackJackAnswer.Count == 0)
                    {
                        GameCurrBlackJackAnswer.Push(new(CurrUser, Response.ToLower()));
                    }
                }
            }
        }

        #endregion
    }
}
