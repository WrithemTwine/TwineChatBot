using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.EFC9.Import;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.Concurrent;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {
        private readonly DataManagerFactory dbContextFactory = new();

        private bool BulkFollowerUpdate;

        /// <summary>
        /// always true to begin one learning cycle
        /// </summary>
        private bool LearnMsgChanged = true;

        private readonly ConcurrentQueue<IEnumerable<Follow>> followsQueue = new();

        private readonly string DefaulSocialMsg = LocalizedMsgSystem.GetVar(Msg.MsgDefaultSocialMsg);
        private DateTime CurrStreamStart { get; set; } = default;

        internal event EventHandler<OnBulkFollowersAddFinishedEventArgs> OnBulkFollowersAddFinished;
        internal event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionUpdated;

        internal DataManagerSQLAsync()
        {
            if (!OptionFlags.EFCDataImportedDataGram)
            {
                bool LogStatus = OptionFlags.LogBotStatus;  // save current logging status

                OptionFlags.LogBotStatus = true; // force logging operations to status during import

                using var context = BuildDataContext();

                ImportDataSources importDataSources = new(); // load the primary database data
                importDataSources.ConvertData(context, this); // convert data loaded from main and multilive data files
                context.SaveChanges(true);

                OptionFlags.LogBotStatus = LogStatus; // restore preferred log status after import
                OptionFlags.EFCDataImportedDataGram = true;
            }

            var initialcontext = BuildDataContext();
            initialcontext.Database.EnsureCreated();
            initialcontext.SaveChanges(true);
            initialcontext.Dispose();

            try
            {
                using var context1 = BuildDataContext();
                context1.Database.Migrate();
                context1.SaveChanges();
            }
            catch { /* ignore */ }

            GUIContext = BuildDataContext();
        }

        private SQLDBContext BuildDataContext()
        {
            return dbContextFactory.CreateDbContext();
        }

        private void ClearDataContext(SQLDBContext context)
        {
            context.Dispose();
        }
    }
}
