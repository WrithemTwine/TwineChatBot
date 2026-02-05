using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

namespace TestStreamerBot.TestAds
{
    public class TestAdNotifications
    {
        internal event EventHandler<NotifyAdSoonEventArgs> NotifyAdSoon;
        internal event EventHandler<NotifyAdStartedEventArgs> NotifyAdStarted;
        internal event EventHandler<EventArgs> NotifyAdEnded;

        internal List<string> Notifications = [];

        public TestAdNotifications()
        {
            NotifyAdSoon += (s, e) =>
            {
                Notifications.Add($"{DateTime.Now} - [Ad Soon] Ad starting in {e.SecondsUntilAd} seconds for duration {e.AdDuration} seconds.");
            };
            NotifyAdStarted += (s, e) =>
            {
                Notifications.Add($"{DateTime.Now} - [Ad Started] Ad started for duration {e.AdDuration} seconds.");
            };
            NotifyAdEnded += (s, e) =>
            {
                Notifications.Add($"{DateTime.Now} - [Ad Ended] Ad has ended.");
            };
        }

        [Fact]
        public async Task TestAdNotificationMessages()
        {
           List<CurrAdSchedule> AdStatus =
                 [
                     new CurrAdSchedule(1, DateTime.MinValue, DateTime.Now.AddSeconds(15), 10, DateTime.Now.ToLocalTime(), 0 ),
                    new CurrAdSchedule(0, DateTime.MinValue, DateTime.Now.AddSeconds(80), 10, DateTime.Now.ToLocalTime(), 0 )
                 ];

            List<CurrAdSchedule> Schedules = [];

            int TwitchAdsNotifySeconds = 10;

            DateTime NextAdCheck = DateTime.Now;
            CurrAdSchedule CurrAd = null;
            DateTime CurrTime;
            DateTime LastAdAtNotify = DateTime.MinValue;
            bool AdSoonNotify = false, AdStartNotify = false;

            while (AdStatus.Count > 0)
            {
                CurrTime = DateTime.Now;

                // only check for ads while there isn't a current ad pending
                if (CurrTime >= NextAdCheck)
                {
                    LogWriter.DebugLog("AdNotificationThread", DebugLogTypes.TwitchBots, "Checking for upcoming or active ads to notify.");
                    if (AdStatus != null)
                    {
                        if (AdStatus.Count > 0)
                        {
                            //Notifications.Add($"{DateTime.Now} - [Ad Check] Found {AdStatus.Count} scheduled ads.");
                            Schedules.Clear();

                            Schedules.AddRange(
                                        AdStatus
                                        //.Where(n => !string.IsNullOrEmpty(n.NextAdAt))  // can be empty if no ad is scheduled => ignore these entries
                                        //.Select(c => c)
                                        //.ToList()
                                        //.ConvertAll(a => new CurrAdSchedule(a))
                                        //    ,
                                        //(Schedules, a) => Schedules
                                        //    .Where((b) => b.NextAdAt == a.NextAdAt)
                                        //    .Select((c) => c)
                                        //    .Any()
                                        );
                            Schedules = [.. Schedules.OrderBy((a) => a.NextAdAt)];

                            //Notifications.Add($"{DateTime.Now} - [Ad Check] Next ad scheduled at {Schedules.First().NextAdAt} for duration {Schedules.First().Duration.TotalSeconds} seconds.");
                            CurrAd = Schedules.First(); // make sure we have the earliest ad
                            NextAdCheck = CurrAd.NextAdAt + CurrAd.Duration; // wait until current ad ends to check again

                            //Notifications.Add($"{DateTime.Now} - [Ad Check] Next ad check scheduled at {NextAdCheck}.");

                            if (LastAdAtNotify != CurrAd.NextAdAt)
                            {
                                //Notifications.Add($"{DateTime.Now} - [Ad Check] Snoozed ad from {LastAdAtNotify} to {CurrAd.NextAdAt}.");
                                AdSoonNotify = false;
                            }
                        }
                    }
                    else
                    {
                        NextAdCheck = CurrTime.AddSeconds(2); // check every 2 seconds
                    } // check every 30 seconds or wait until the next ad time
                }

                // a pending ad will run soon
                if (CurrAd != null)
                {
                    /* 
Ad chronology:
                                                                    between_ads     AdSoon  Waiting     AdStarted       AdEnd
-> DateTime.MinValue;                                               t               f       f           f               f
(CurrTime = (DateTime.Now +NotifySeconds) >= NextAdTime) == false;  t               f       f           f               f
(CurrTime = (DateTime.Now +NotifySeconds) >= NextAdTime) == true;   f               t       f           f               f
(CurrTime = DateTime.Now) >= NextAdTime == false && NotifyAdSoon;   f               f       t           f               f
(CurrTime = DateTime.Now) >= NextAdTime == true && NotifyAdSoon;    f               f       f           t               f
(CurrTime = DateTime.Now+AdDuration) >= NextAdTime == false;        f               f       f           t               f

(CurrTime = DateTime.Now+AdDuration) >= NextAdTime == true;         f               f       f           t               t
                                            (the AdStarted check and AdEnd check both satisfy, without using a progress flag)

                    DateTime >= NextAdTime+AdDuration: can be false for AdEnd and true for AdStarted
                    */

                    if (!AdSoonNotify && CurrTime.AddSeconds(TwitchAdsNotifySeconds) >= CurrAd.NextAdAt)
                    // first check if we need to notify ads are starting soon - should occur first chronologically
                    {
                        //Notifications.Add($"{DateTime.Now} - [Ad Soon Check] Notifying ad starting soon for ad at {CurrAd.NextAdAt}.");
                        LastAdAtNotify = CurrAd.NextAdAt;
                        AdSoonNotify = true;
                        NotifyAdSoon?.Invoke(this, new(TwitchAdsNotifySeconds, CurrAd.Duration));
                    }
                    else if (!AdStartNotify && CurrTime >= CurrAd.NextAdAt)
                    // check if now is after the ad should start; this occurs second chronologically
                    {
                        //Notifications.Add($"{DateTime.Now} - [Ad Started Check] Notifying ad has started for ad at {CurrAd.NextAdAt}.");
                        AdStartNotify = true;
                        NotifyAdStarted?.Invoke(this, new(CurrAd.Duration));
                    }
                    else if (CurrTime >= CurrAd.GetAdEnd)
                    // check if now is after the ad should end; this occurs third chronologically
                    {
                        //Notifications.Add($"{DateTime.Now} - [Ad Ended Check] Notifying ad has ended for ad at {CurrAd.NextAdAt}.");
                        NotifyAdEnded?.Invoke(this, new());
                        NextAdCheck = CurrTime;
                        AdSoonNotify = false;
                        AdStartNotify = false;
                        AdStatus.Remove(CurrAd); // remove the ad from the list since it's over
                        CurrAd = null; // reset the CurrAd to get the next ad
                        LastAdAtNotify = DateTime.MinValue; // reset the ad soon notify time
                    }

                    if (CurrAd!=null && CurrAd.SnoozeCount > 0 && CurrTime.AddSeconds(TwitchAdsNotifySeconds) >= CurrAd.NextAdAt)
                    { // a snooze will shift the ad time by 5 minutes later, check for a snooze before notifying => reset the CurrAd and do it again
                        NextAdCheck = CurrTime.AddSeconds(1); // check again in 1 second
                        //Notifications.Add($"{DateTime.Now} - [Ad Snooze Check] Snooze available for ad at {CurrAd.NextAdAt}, snoozing now.");

                        //Notifications.Add($"{DateTime.Now} - [Ad Snooze] Snoozing ad scheduled at {CurrAd.NextAdAt}.");
                        //Notifications.Add($"{DateTime.Now} - [Ad Snooze] Snooze count before snooze: {CurrAd.SnoozeCount}.");
                        //Notifications.Add($"{DateTime.Now} - [Ad Snooze] Pushing ad time back by 15 seconds.");
                        AdStatus[0].NextAdAt = CurrAd.NextAdAt.AddSeconds(15); // simulate a snooze by pushing the ad time back by 15 seconds
                        AdStatus[0].SnoozeCount--; // decrease the snooze count by 1
                    }
                }

                await Task.Delay(1000); // wait between checks
            }

          Assert.NotEmpty(Notifications);
        }
    }
}
