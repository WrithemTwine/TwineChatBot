using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;

namespace StreamerBotLib.MachineLearning
{
    public class LearnedMessagesPrimer
    {
        private static string[] SpamBotMsgs =
        {
            "Best followers, primes and viewers on mountviewers.com(remove the space)",
            "Wanna become famous? Buy followers, primes and views on bigfollows .com",
            "Best followers, primes and viewers on mystrm .store (remove the space)"
        };

        public static List<LearnedMessage> PrimerList
        {
            get
            {
                List<LearnedMessage> data = new();
                data.AddRange(LearnedMessage.BuildList(SpamBotMsgs, MsgTypes.InstantBanSpam));

                return data;
            }
        }

        public static List<BanReason> BanReasonList
        {
            get
            {
                return new()
                {
                    new() { MsgType = MsgTypes.Allow, Reason = BanReasons.None },
                    new() { MsgType = MsgTypes.InstantBanHateSpeech, Reason = BanReasons.RacismOrHate },
                    new() { MsgType = MsgTypes.InstantBanSpam, Reason = BanReasons.UnsolicitedSpam },
                    new() { MsgType = MsgTypes.Questionable, Reason = BanReasons.Harrassment },
                    new() { MsgType = MsgTypes.UnidentifiedChatInput, Reason = BanReasons.None }
                };
            }
        }

        public static List<BanViewerRule> BanViewerRulesList
        {
            get
            {
                List<BanViewerRule> output = new();

                foreach (ViewerTypes V in Enum.GetValues(typeof(ViewerTypes)))
                {
                    foreach (MsgTypes M in Enum.GetValues(typeof(MsgTypes)))
                    {
                        BanViewerRule rule = new()
                        {
                            ViewerType = V,
                            MsgType = M,
                        };

                        rule.ModAction = rule.MsgType switch
                        {
                            MsgTypes.InstantBanHateSpeech => V == ViewerTypes.Broadcaster ? ModActions.Timeout : ModActions.Ban,
                            MsgTypes.InstantBanSpam => V <= ViewerTypes.Mod ? ModActions.Timeout : ModActions.Ban,
                            MsgTypes.Questionable => V <= ViewerTypes.VIP ? ModActions.Allow : ModActions.Timeout,
                            MsgTypes.Allow => ModActions.Allow,
                            MsgTypes.UnidentifiedChatInput => ModActions.Allow,
                            _ => throw new NotImplementedException(),
                        };

                        rule.TimeoutSeconds = rule.ModAction switch
                        {
                            ModActions.Allow => "0",
                            ModActions.Ban => "0",
                            ModActions.Timeout => (V is > ViewerTypes.Broadcaster and < ViewerTypes.VIP) ? "0" : "1800",
                            _ => throw new NotImplementedException()
                        };

                        output.UniqueAdd(rule);
                    }
                }

                return output;
            }
        }
    }
}
