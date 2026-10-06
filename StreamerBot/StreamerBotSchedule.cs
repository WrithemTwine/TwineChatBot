using StreamerBotLib.Models.Schedule;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region Schedule_Twitch

        private void Schedule_ListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScheduleData.UniqueAdd((sender as ListView).DataContext as ScheduleBase);
        }

        private void Schedule_SentButton_Click(object sender, RoutedEventArgs e)
        {
            Button curr = sender as Button;
            ProcessSchedule(curr.DataContext as ScheduleConfig);
        }

        private void ProcessSchedule(ScheduleConfig currSched)
        {
            if (OptionFlags.ScheduleUseSchedule)
            {
                currSched.SentDay = true;

                Controller.SendSchedule(currSched);
            }
        }


        #region Thread Check

        private List<ScheduleBase> ScheduleData = [];

        public void CheckSchedule()
        {
            foreach (var data in from item in ScheduleData
                                 let data = item.GetCurrDayConfig(DateTime.Now)
                                 where data != null && !string.IsNullOrEmpty(data.Title) && !string.IsNullOrEmpty(data.CategoryName) && !data.SentDay
                                 select data)
            {
                ProcessSchedule(data);
            }
        }

        public void ResetSchedule()
        {
            foreach (var data in from item in ScheduleData
                                 select item)
            {
                data.ResetDay();
            }
        }

        #endregion


        #endregion
    }
}
