using StreamerBotLib.Properties;
using StreamerBotLib.Static;

namespace TestStreamerBot
{
    // Run sequentially to avoid static OptionFlags/Settings interference across tests
    [Collection("Sequential")]
    public class TestFormatData
    {
        private void ResetFormatSettings()
        {
            var s = Settings.Default;
            // ensure relevant settings are in a known state before each test
            s.FormatTimeTotalHours = false;
            s.FormatTimeFullFormatFullHours = false;
            s.FormatTimeFullFormatHoursFull = false;
            s.FormatTimeTotalTime = false;
            s.FormatTimeIncludeSeconds = false;
            s.TwitchAdsNotifyTimeFormatTotalTime = false;
            s.Save();
        }

        [Fact]
        public void FormatTimes_ZeroTime_TwitchAdsNotifyTrue_ReturnsNoTimeAvailable()
        {
            ResetFormatSettings();
            Settings.Default.TwitchAdsNotifyTimeFormatTotalTime = true;
            Settings.Default.Save();

            var ts = TimeSpan.Zero;
            var result = FormatData.FormatTimes(ts);

            Assert.Equal("no time available", result);
        }

        [Fact]
        public void FormatTimes_NonZeroTime_TwitchAdsNotifyTrue_ReturnsNoTimeAvailable()
        {
            ResetFormatSettings();
            Settings.Default.TwitchAdsNotifyTimeFormatTotalTime = true;
            //Settings.Default.FormatTimeIncludeSeconds = true;
            Settings.Default.Save();

            var ts = TimeSpan.FromSeconds(95);
            var result = FormatData.FormatTimes(ts);

            Assert.Equal("1 minute, 35 seconds", result);
        }

        [Fact]
        public void FormatTimes_ZeroTime_TotalHours_ReturnsZeroHours()
        {
            ResetFormatSettings();
            Settings.Default.FormatTimeTotalHours = true;
            Settings.Default.Save();

            var ts = TimeSpan.Zero;
            var result = FormatData.FormatTimes(ts);

            // Expect the "total hours" textual representation, e.g. "0 hours"
            Assert.Equal("0 hours", result);
        }

        [Fact]
        public void FormatTimes_ZeroTime_FullFormatHoursFull_ReturnsTotalHoursOrTotaltime()
        {
            ResetFormatSettings();
            Settings.Default.FormatTimeFullFormatHoursFull = true;
            Settings.Default.Save();

            var ts = TimeSpan.Zero;
            var result = FormatData.FormatTimes(ts);

            // LocalizedMsgSystem.GetVar(MsgVars.or) => "{0} or {1}"
            // totalhours => "0 hours", totaltime => "no time available"
            Assert.Equal("0 hours or no time available", result);
        }

        [Fact]
        public void FormatTimes_NonZeroTime_ReturnsCompositeTotalTime()
        {
            ResetFormatSettings();
            // Request to return "total time" (the composed parts like "1 hour, 30 minutes")
            Settings.Default.FormatTimeTotalTime = true;
            Settings.Default.Save();

            // 1 hour 30 minutes
            var ts = TimeSpan.FromMinutes(90);
            var result = FormatData.FormatTimes(ts);

            // Expect "1 hour, 30 minutes"
            Assert.Equal("1 hour, 30 minutes", result);
        }
    }
}
