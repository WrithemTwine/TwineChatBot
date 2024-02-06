using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Static;

using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;

using static StreamerBotLib.DataSQL.Import.DataSource;

using MultiDataSource = StreamerBotLib.DataSQL.Import.Multi.DataSource;

namespace StreamerBotLib.DataSQL.Import
{
    internal class ImportDataSources : BaseDataManager
    {
        private static readonly string DataFileXML = "ChatDataStore.xml";
        private DataSource _DataSource;

        private static readonly string MultiDataFileXML = "MultiChatbotData.xml";
        private readonly MultiDataSource _MultiDataSource;

        public ImportDataSources(bool useMainFile) : base(useMainFile ? DataFileXML : MultiDataFileXML)
        {
            if (useMainFile)
            {
                LoadData();
            }
            else
            {
                MultiLoadData();
            }
        }

        /// <summary>
        /// Load the data source and populate with default data; if regular data source is corrupted, attempt to load backup data.
        /// </summary>
        private void LoadData()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Loading the database.");

            void LoadFile(string filename)
            {
                lock (GUIDataManagerLock.Lock)
                {
                    if (!File.Exists(filename))
                    {
                        _DataSource.WriteXml(filename);
                    }

                    _ = _DataSource.ReadXml(new XmlTextReader(filename), XmlReadMode.DiffGram);

                    foreach (CommandsRow c in _DataSource.Commands.Select())
                    {
                        if (DBNull.Value.Equals(c["IsEnabled"]))
                        {
                            c["IsEnabled"] = true;
                        }
                    }
                }
                OptionFlags.DataLoaded = true;
            }

            TryLoadFile((xmlfile) => LoadFile(xmlfile));

        }

        /// <summary>
        /// Format the file name to handle debug path locations, otherwise ignored in release versions.
        /// </summary>
        /// <param name="fileName">Name of the file to format, prepend debug directory.</param>
        /// <returns>File path, whether in debug directory or relative filename in current release directory - based on Current Working Directory.</returns>
        private string FormatFileName(string fileName)
        {
            return
#if DEBUG
            // add specific directory location for debug purposes, ignore for release
            Path.Combine(Directory.GetCurrentDirectory(),
#endif
                fileName
#if DEBUG
                )
#endif
                ;
        }

        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        public void MultiLoadData()
        {
            _MultiDataSource.Clear();
            void LoadFile(string filename)
            {
                lock (_MultiDataSource)
                {
                    if (!File.Exists(filename))
                    {
                        _MultiDataSource.WriteXml(filename);
                    }

                    _ = _MultiDataSource.ReadXml(new XmlTextReader(filename), XmlReadMode.DiffGram);

                }
                OptionFlags.MultiDataLoaded = true;
            }

            TryLoadFile((xmlfile) => LoadFile(xmlfile));

            try
            {
                _MultiDataSource.AcceptChanges();
            }
            catch (ConstraintException)
            {
                _MultiDataSource.EnforceConstraints = false;

                //foreach (DataTable table in _MultiDataSource.Tables)
                //{
                //    List<DataRow> UniqueRows = [];
                //    List<DataRow> DuplicateRows = [];

                //    foreach (DataRow datarow in table.Rows)
                //    {
                //        if (!UniqueRows.UniqueAdd(datarow, new DataRowEquatableComparer()))
                //        {
                //            DuplicateRows.Add(datarow);
                //        }
                //    }

                //    DuplicateRows.ForEach(r => r.Delete());
                //}

                _MultiDataSource.AcceptChanges();
                _MultiDataSource.EnforceConstraints = true;
            }
        }



    }
}
