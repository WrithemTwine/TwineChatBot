using StreamerBotLib.Data.MultiLive;
using StreamerBotLib.Static;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamerBotLib.Data
{
    /*
     Manages all input and output data for the datamanagers, to organize and manage. Trying to prevent the 
     the "corrupted index 13" exception, which crashes the bot.
     */
    public class DataController
    {
        private static DataManager DataManager = new();
        private static MultiDataManager MultiDataManage = new();

        private ConcurrentQueue<Task> Tasks { get; set; } = new();


        public DataController()
        {
            ThreadManager.CreateThreadStart(() => { ManageTasksDataManager(); });
        }

        #region DataManager
        private async void ManageTasksDataManager()
        {
            while (OptionFlags.ActiveToken)
            {
                if(Tasks.TryDequeue(out Task task))
                {
                   await task;
                }

                Thread.Sleep(Tasks.IsEmpty ? 5000 : 2000);
            }
        }

        public void SetData(string TableName, string Key)
        {

        }

        public void GetData()
        {

        }


        public static string[] GetTableNames() => new List<string>(from DataTable d in new DataSource().Tables
                                                                              select d.TableName).ToArray();

        #endregion

        #region Multi DataManager

        private void ManageTasksMultiDataManager()
        {

        }

        #endregion
    }
}
