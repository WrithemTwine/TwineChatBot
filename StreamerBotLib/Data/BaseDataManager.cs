using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace StreamerBotLib.Data
{
    public class BaseDataManager
    {
        protected readonly string DataFileName;
        protected readonly string BackupDataFileXML;

        protected readonly Queue<Task> SaveTasks = new();
        protected bool SaveThreadStarted = false;
        protected const int SaveThreadWait = 10000;
        protected const int BackupSaveIntervalMins = 15;
        protected const int BackupHrInterval = 60 / BackupSaveIntervalMins;
        protected const int MaxLogLength = 8000;
        protected int BackupSaveToken = 0;

        public BaseDataManager(string DataFileXMLName)
        {
            DataFileName = DataFileXMLName;
            BackupDataFileXML = $"Backup_{DataFileXMLName}";

            BackupSaveToken = DateTime.Now.Minute / BackupSaveIntervalMins;

        }

        protected void BeginLoadData(DataTableCollection dataTables)
        {
            foreach (DataTable table in dataTables)
            {
                table.BeginLoadData();
            }

        }

        protected void TryLoadFile(Action<string> LoadFile)
        {
            try // try to catch any exception when loading the backup working file, incase there's an issue loading the backup file
            {
                try // try the regular working file
                {
                    LoadFile(DataFileName);
                }
                catch (Exception ex) // catch if exception loading the data file, e.g. file corrupted from system crash
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    File.Copy(DataFileName, $"Failed_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Path.GetFileName(DataFileName)}");
                    LoadFile(BackupDataFileXML);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        protected void EndLoadData(DataTableCollection dataTables)
        {
            foreach (DataTable table in dataTables)
            {
                table.EndLoadData();
            }

        }

        protected void SaveData(Action<Stream, XmlWriteMode> WriteXML, Action<string, XmlWriteMode> StringWriteXML, object Lock, Action<MemoryStream> TestDataSource, bool MultiData = false)
        {
            int CurrMins = DateTime.Now.Minute;
            bool IsBackup = CurrMins >= BackupSaveToken * BackupSaveIntervalMins && CurrMins < (BackupSaveToken + 1) % BackupHrInterval * BackupSaveIntervalMins;

            if (IsBackup)
            {
                lock (BackupDataFileXML)
                {
                    BackupSaveToken = (CurrMins / BackupSaveIntervalMins) % BackupHrInterval;

                }
            }
            if (!SaveThreadStarted) // only start the thread once per save cycle, flag is an object lock
            {
                SaveThreadStarted = true;
                ThreadManager.CreateThreadStart(PerformSaveOp, ThreadWaitStates.Wait, ThreadExitPriority.Low); // need to wait, else could corrupt datafile
            }

            lock (SaveTasks) // lock the Queue, block thread if currently save task has started
            {
                SaveTasks.Enqueue(new(() =>
                {
                    lock (Lock)
                    {
                        try
                        {
                            MemoryStream SaveDataMS = new();  // new memory stream
                            WriteXML(SaveDataMS, XmlWriteMode.DiffGram); // save the database to the memory stream
                            TestDataSource(SaveDataMS);
                            StringWriteXML(DataFileName, XmlWriteMode.DiffGram); // write the valid data to file

                            // determine if current time is within a certain time frame, and perform the save
                            if (IsBackup && OptionFlags.IsStreamOnline || MultiData)
                            {
                                // write backup file
                                StringWriteXML(BackupDataFileXML, XmlWriteMode.DiffGram); // write the valid data to file
                            }
                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                        }
                    }
                }));
            }

        }

        protected void PerformSaveOp()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Managed database save data.");
#endif

            if (OptionFlags.ActiveToken) // don't sleep if exiting app
            {
                Thread.Sleep(SaveThreadWait);
            }

            lock (SaveTasks) // in case save actions arrive during save try
            {
                if (SaveTasks.Count >= 1)
                {
                    SaveTasks.Dequeue().Start(); // only run 1 of the save tasks
                }
                SaveTasks.Clear();
            }
            SaveThreadStarted = false; // indicate start another thread to save data
        }


    }
}
