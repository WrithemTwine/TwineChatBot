using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;

namespace StreamerBotLib.Systems.CurrencyGames
{
    /// <summary>
    /// Implements the BlackJack card game, in text chat
    /// </summary>
    internal class BlackJack
    {
        public const int BlackJackWin = 21;
        private int HouseStandValue = OptionFlags.GameBlackJackHouseStands;

        private PlayGameUserWager<PlayingCardFrench, PlayingCardSuit> HouseCards { get; set; } = new(null, 0);

        private PlayingCards<PlayingCardFrench, PlayingCardSuit> PlayDeck { get; set; }

        private List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> GameUsers { get; set; } = new();

        internal BlackJack() { }

        internal void AddUser(LiveUser CurrUser, int Wager)
        {
            GameUsers.Add(new(CurrUser, Wager));
        }

        internal void BuildDeck()
        {
            void SetupCard()
            {
                foreach (var user in GameUsers)
                {
                    UserWantsCard(user.Player);
                }
                UserWantsCard();
            }

            int Count = (int)Math.Ceiling(GameUsers.Count / 7.0);
            Count = Count > 0 ? Count : 1;
            PlayDeck = new(Count);

            SetupCard(); // deal round 1
            SetupCard(); // deal round 2
        }

        internal string GetUserCard(LiveUser CurrUser = null)
        {
            string Message;
            PlayGameUserWager<PlayingCardFrench, PlayingCardSuit> CurrCards = CurrUser == null ? HouseCards : GameUsers.Find((s) => s.Player == CurrUser);

            if (CurrCards == null)
            {
                Message = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackNoPlayer);
            }
            else
            {
                Message = VariableParser.ParseReplace(
                    LocalizedMsgSystem.GetVar(
                        CurrUser == null ?
                        PlayCardBlackJack.BlackJackHouseCardMsg :
                        PlayCardBlackJack.BlackJackCardMessage
                        ),
                   VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                    new( MsgVars.cards, CurrCards.CardItems.Replace("_","") ),
                    new( MsgVars.value, CurrCards.CardCount.ToString())
                }));
            }

            return Message;
        }

        internal int GetUserCardValue(LiveUser CurrUser)
        {
            return GameUsers.Find((s) => s.Player == CurrUser).CardCount;
        }

        private static int GetCardValue(int CardCount, PlayingCard<PlayingCardFrench, PlayingCardSuit> card)
        {
            return card.PlayingCardValue switch
            {
                PlayingCardFrench.A => CardCount > 10 ? 1 : 11,
                PlayingCardFrench.J or PlayingCardFrench.Q or PlayingCardFrench.K => 10,
                _ => (int)card.PlayingCardValue
            };
        }

        internal bool CheckUserStatus(LiveUser CurrUser)
        {
            return GameUsers.Find((s) => s.Player == CurrUser).CardCount < BlackJackWin;
        }

        internal void UserWantsCard(LiveUser CurrUser = null)
        {
            int CountCards(List<PlayingCard<PlayingCardFrench, PlayingCardSuit>> cards)
            {
                int TotalValue = 0;
                List<PlayingCard<PlayingCardFrench, PlayingCardSuit>> hasAce = new();
                foreach (var C in cards)
                {
                    if (C.PlayingCardValue == PlayingCardFrench.A)
                    {
                        hasAce.Add(C);
                    }
                    else
                    {
                        TotalValue += GetCardValue(TotalValue, C);
                    }
                }

                foreach (var A in hasAce)
                {
                    TotalValue += GetCardValue(TotalValue + hasAce.Count - 1, A);
                }

                return TotalValue;
            }

            var U = CurrUser == null ? HouseCards : GameUsers.Find((s) => s.Player == CurrUser);
            var GotCard = PlayDeck.DealCard();

            U.Cards.Add(GotCard);
            U.CardCount = CountCards(U.Cards);
            U.CardItems += GotCard.ToString();
        }

        /// <summary>
        /// Checks if House won the hand and "HouseWinsTie" flag is checked-per user setup option.
        /// </summary>
        /// <returns><code>true</code> the House has 21; <code>false</code> the House is under 21.</returns>
        internal bool CheckHouseWin()
        {
            return HouseCards.CardCount == BlackJackWin;
        }

        /// <summary>
        /// Perform the House deal endgame.
        /// </summary>
        /// <returns>The result of the House's play.</returns>
        internal string HousePlay()
        {
            while (HouseCards.CardCount < HouseStandValue)
            {
                UserWantsCard();
            }

            string Message;
            if (HouseCards.CardCount < BlackJackWin)
            {
                Message = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackHouseStands);
            }
            else if (HouseCards.CardCount == BlackJackWin)
            {
                Message = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackHouseHits21);
            }
            else
            {
                Message = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackHouseBusts);
            }

            CalculatePayout();

            return $"{GetUserCard()} {Message}";
        }

        private void CalculatePayout()
        {
            foreach (PlayGameUserWager<PlayingCardFrench, PlayingCardSuit> U in GameUsers)
            {
                if (U.CardCount > BlackJackWin)
                {
                    U.ResultMessage = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackBust);
                }
                else if (HouseCards.CardCount <= BlackJackWin && U.CardCount <= HouseCards.CardCount)
                {
                    if (U.CardCount == HouseCards.CardCount)
                    {
                        U.ResultMessage = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackDraw);
                        U.Payout = U.Wager;
                    }
                    else if (U.CardCount < HouseCards.CardCount)
                    {
                        U.ResultMessage = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackHouseWin);
                    }
                }
                else
                {
                    U.Win = true;
                    if (U.CardCount == BlackJackWin)
                    {
                        U.ResultMessage = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJack21Win);

                        if (U.Cards.Count == 2)
                        {
                            U.Payout = U.Wager + U.Wager * (OptionFlags.GameBlackJackPayoutDealt21 / 100.0);
                        }
                        else
                        {
                            U.Payout = U.Wager + U.Wager * (OptionFlags.GameBlackJackPayoutReach21 / 100.0);
                        }
                    }
                    else
                    {
                        U.ResultMessage = LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackWinOverHouse);
                        U.Payout = U.Wager + U.Wager * (OptionFlags.GameBlackJackPayoutUnder21 / 100.0);
                    }
                }
                string Pay = VariableParser.ParseReplace(LocalizedMsgSystem.GetVar(PlayCardBlackJack.BlackJackPayout),
                    VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.payout, U.Payout.ToString("N0"))
                    }));
                U.ResultMessage = $"{U.Player.UserName}, {GetUserCard(U.Player)} {U.ResultMessage} {Pay}";
            }
        }

        internal List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> PayoutPlayers()
        {
            return GameUsers;
        }
    }
}
