using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

namespace StreamerBotLib.Models.Schedule
{
    public class ScheduleTwitch : ScheduleBase
    {
        public ScheduleTwitch()
        {
            Platform = Platform.Twitch;
            Load();
        }

        public override void Save() => OptionFlags.ScheduleTwitch = PrepareSaveData();

        private void Load() => PrepareLoadData(OptionFlags.ScheduleTwitch);
    }
}
