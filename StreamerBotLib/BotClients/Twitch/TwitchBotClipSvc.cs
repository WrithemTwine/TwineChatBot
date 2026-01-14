#define SIMPLE_CLIP_BOT

#if SIMPLE_CLIP_BOT

using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Diagnostics.CodeAnalysis;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Clips.CreateClip;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Interfaces;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotClipSvc : TwitchBotsBase
    {
        private readonly TwitchTokenBot tokenBot;
        private ITwitchAPI twitchAPI => tokenBot?.StreamerHelixApi;
        private Action _rePerformAction; // used to hold an action to re-perform after a token refresh

        private int QueryCountPerRequest = 100;

        private Thread GetClipsThread;

        public event EventHandler<OnNewClipsDetectedArgs> OnNewClipFound;
        public event EventHandler<ExpiredTokenEventArgs> AccessTokenUnauthorized;

        internal TwitchBotClipSvc(TwitchTokenBot TokenBot)
        {
            BotClientName = Bots.TwitchClipBot;
            tokenBot = TokenBot;

            tokenBot.StreamerAccessTokenChanged += TokenBot_StreamerAccessTokenChanged;
            AccessTokenUnauthorized += ClipThread_AccessTokenUnauthorized;
        }

        private void TokenBot_StreamerAccessTokenChanged(object sender, EventArgs e)
        {
            if (IsActive == true)
            {
                //Start();
            }

            _rePerformAction?.Invoke();
            _rePerformAction = null;
        }

        private void ClipThread_AccessTokenUnauthorized(object sender, ExpiredTokenEventArgs e)
        {
            LogWriter.DebugLog("ClipThread_AccessTokenUnauthorized", DebugLogTypes.TwitchClipBot, "Checking tokens.");
            _rePerformAction = e.RePerformAction;
            tokenBot.CheckToken();
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        public override Task StartBot()
        {
            return Task.Run(() =>
            {
                if (IsActive != true)
                {
                    IsActive = true;

                    LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Starting clip bot.");

                    GetClipsThread = ThreadManager.CreateThread("TwitchBotClipSvc.ProcessClips", ManageClipsThread);
                    GetClipsThread.Start();
                }

                InvokeBotStarted();
            });
        }
    
        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override Task StopBot()
        {
            return Task.Run(() =>
            {
                if (IsActive == true)
                {
                    LogWriter.DebugLog("StopBot", DebugLogTypes.TwitchClipBot, "Stopping clip bot.");
                    IsActive = false;
                    InvokeBotStopped();
                    GetClipsThread = null; // clear thread reference
                }
            });
        }
        public override Task<bool> ExitBot()
        {
            return Task.Run(() =>
            {
                if (IsActive == true)
                {
                    LogWriter.DebugLog("ExitBot", DebugLogTypes.TwitchClipBot, "Now stopping and exiting bot.");
                    StopBot();
                    GetClipsThread.Join();
                }
                return true;
            });
        }



        #region Clip Ops

        private string simpleLock = "lockobject";
        private List<Clip> KnownClips = [];
        private DateTime currCheck;
        private DateTime startedAt;

        private void ManageClipsThread()
        {
            ProcessClips().Wait();
        }

        private async Task ProcessClips()
        {
            lock (simpleLock)
            {
                startedAt = DateTime.Now; // set the start time for clip checking to now, new 'latest' clips in this window
                currCheck = DateTime.Now;
            }

            var resultClips = await GetAllClipsAsync(); // get all existing clips

            if (resultClips != null) // proceed only if we got clips = no token issues (may be 0 entries)
            {
                KnownClips = resultClips;

                OnNewClipFound?.Invoke(this, new() { AllClips = true, Clips = KnownClips }); // initial load of clips

                while (OptionFlags.ActiveToken && IsActive == true)
                {
                    if(DateTime.Now >= currCheck.AddSeconds(OptionFlags.TwitchFrequencyClipTime)) // only check at specific intervals
                    {  // time to check for new clips
                        var latestClips = await GetLatestClipsAsync(); // get the current stream clips
                        latestClips.RemoveAll((c) => KnownClips.Contains(c, new ClipsComparer()));  // new clips are those not in known list

                        if (latestClips.Count > 0)
                        {
                            KnownClips.AddRange(latestClips); // add new clips to known list, won't be found as new clips next time
                            OnNewClipFound?.Invoke(this, new() { AllClips = false, Clips = latestClips }); // notify of new clips
                        }

                        lock (simpleLock)
                        {
                            currCheck = DateTime.Now;
                        }
                    }

                    Task.Delay(500).Wait(); // waiting briefly to avoid tight loop, ensures app doesn't hang if user wants to close
                }
            }

            IsActive = false; // if we reach here, we are stopping the clip check thread
        }

        public async Task CreateClipAsync()
        {
            if (IsActive == true)
            {
                LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, "Creating a new clip.");

                try
                {
                    CreatedClipResponse clipResponse = await twitchAPI.Helix.Clips.CreateClipAsync(OptionFlags.TwitchStreamerUserId);
                    LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, $"Clip creation response received, waiting to verify clip creation.");

                    await Task.Delay(15000); // wait for clip to be created before checking for the clip

                    LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, $"Checking for created clip now.");
                    GetClipsResponse data = null;
                    int x = 0;

                    if (clipResponse != null && clipResponse.CreatedClips.Length > 0)
                    {
                        LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, $"Created clip ID: {clipResponse.CreatedClips[0].Id}, starting verification loop.");
                        do
                        {
                            data = await twitchAPI.Helix.Clips.GetClipsAsync([..clipResponse.CreatedClips.Select(c => c.Id)], startedAt: startedAt);

                            LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, $"Checking for created clip, attempt {x+1}.");
                            if (data != null && data.Clips.Length > 0)
                            {
                                LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, $"Created clip found: {data.Clips[0].Id}");
                                OnNewClipFound?.Invoke(this, new() { AllClips = false, Clips = [..data.Clips] }); // notify of new clip
                            }
                            else
                            {
                                await Task.Delay(x*1200); // wait longer each time and try again
                                LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, $"Created clip not found yet, retrying... Attempt {x+1}");
                            }
                        } while (data == null && x++ < 5);
                    }
                    else
                    {
                        LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, $"No clip creation response received from Twitch.");
                    }
                }
                catch (BadScopeException)
                {
                    LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, "Access token unauthorized, invoking event to refresh token.");
                    AccessTokenUnauthorized?.Invoke(this, new(async () => await CreateClipAsync())); // ignore perform action, thread calls this again later
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "CreateClipAsync");
                }
            }
            else
            {
                LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, "Clip bot is not active, cannot create clip.");
            }
        }

        /// <summary>
        /// Asynchronously retrieves the latest clips for the specified Twitch broadcaster since the time the check loop started - it's usually started when a stream starts.
        /// </summary>
        /// <returns>A list of <see cref="Clip"/> objects representing the latest clips. Returns <see langword="null"/> if the
        /// request fails or if the access token is unauthorized.</returns>
        private async Task<List<Clip>> GetLatestClipsAsync()
        {
            List<Clip> clips = [];
            try
            {
                GetClipsResponse curr = await twitchAPI.Helix.Clips.GetClipsAsync(first: QueryCountPerRequest, broadcasterId: OptionFlags.TwitchStreamerUserId, startedAt: startedAt);
                if (curr.Clips.Length > 0)
                {
                    clips.AddRange(curr.Clips);
                }
                return clips;
            }
            catch (BadScopeException)
            {
                AccessTokenUnauthorized?.Invoke(this, new(null)); // ignore perform action, thread calls this again later
                return null;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "GetAllClipsAsync");
                return null;
            }
        }

        /// <summary>
        /// Retrieves all clips for the specified Twitch broadcaster asynchronously, up to 1000.
        /// </summary>
        /// <returns>List of clips.</returns>
        private async Task<List<Clip>> GetAllClipsAsync()
        {
            string after = null;

            List<Clip> clips = [];

            try
            {
                do
                {
                    GetClipsResponse curr = await twitchAPI.Helix.Clips.GetClipsAsync(first: QueryCountPerRequest,
                                                                                      broadcasterId: OptionFlags.TwitchStreamerUserId,
                                                                                      after: after);
                    after = curr.Pagination.Cursor;
                    clips.AddRange(curr.Clips);
                } while (after != null && clips.Count < 1000);

                return clips;
            }
            catch (BadScopeException)
            {
                AccessTokenUnauthorized?.Invoke(this, new(async () => await GetAllClipsAsync())); // perform action to re-call after token refresh
                return null;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "GetAllClipsAsync");
                return null;
            }

        }

        #endregion

        internal class ClipsComparer : IEqualityComparer<Clip>
        {
            public bool Equals(Clip x, Clip y)
            {
                return (x.CreatedAt == y.CreatedAt) && (x.Id == y.Id);
            }

            public int GetHashCode([DisallowNull] Clip obj)
            {
                return obj.CreatedAt.GetHashCode();
            }
        }
    }
}

#else
using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Clips.CreateClip;
using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotClipSvc : TwitchBotsBase
    {
        private readonly TwitchTokenBot tokenBot;
        public ClipMonitorService ClipMonitorService { get; set; }

        private Action _rePerformAction; // used to hold an action to re-perform after a token refresh

        internal TwitchBotClipSvc(TwitchTokenBot TokenBot)
        {
            BotClientName = Bots.TwitchClipBot;
            tokenBot = TokenBot;

            tokenBot.StreamerAccessTokenChanged += TokenBot_StreamerAccessTokenChanged;
        }

        private void TokenBot_StreamerAccessTokenChanged(object sender, EventArgs e)
        {
            ClipMonitorService.UpdateAccessToken(tokenBot.StreamerApiSettings.AccessToken); // update the clip service with the new token
            if (IsActive == true && !ClipMonitorService.Enabled)
            {
                ClipMonitorService.Start();
            }

            _rePerformAction?.Invoke();
            _rePerformAction = null;
        }

        /// <summary>
        /// Builds the clip service.
        /// </summary>
        private void ConnectClipService()
        {
            if (ClipMonitorService == null)
            {
                LogWriter.DebugLog("ConnectClipService", DebugLogTypes.TwitchClipBot, "Building clip service object.");

                ClipMonitorService = new(tokenBot.StreamerHelixApi, (int)Math.Ceiling(OptionFlags.TwitchFrequencyClipTime));
                ClipMonitorService.SetChannelsById([OptionFlags.TwitchStreamerUserId]);

                ClipMonitorService.AccessTokenUnauthorized += ClipMonitorService_AccessTokenUnauthorized;
            }
        }

        private void ClipMonitorService_AccessTokenUnauthorized(object sender, ExpiredTokenEventArgs e)
        {
            LogWriter.DebugLog("ClipMonitorService_AccessTokenUnauthorized", DebugLogTypes.TwitchClipBot, "Checking tokens.");
            _rePerformAction = e.RePerformAction;
            tokenBot.CheckToken();
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        public override Task StartBot()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (IsActive == null || IsActive == false)
                    {
                        tokenBot.UpdateActiveTokens(BotType.StreamerAccount, true);
                        tokenBot.CheckToken();

                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Starting bot.");

                        ConnectClipService();
                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Starting service.");
                        ClipMonitorService.Start();
                        IsActive = true;
                        InvokeBotStarted();
                    }
                }
                catch (BadRequestException)
                {
                    LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Checking tokens.");
                    tokenBot.CheckToken();
                }
                catch (Exception ex)
                {
                    LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Caught an exception trying to start the bot.");
                    LogWriter.LogException(ex, "StartBot");
                    if (IsActive == false)
                    {
                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Found the bot didn't start, notifying GUI the bot is stopped.");

                        IsActive = false;
                        InvokeBotFailedStart();
                    }
                    else
                    {
                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Determined bot is started, notifying GUI the bot started.");
                        IsActive = true;
                        InvokeBotStarted();
                    }
                }
            });
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override Task StopBot()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (IsActive == true)
                    {
                        LogWriter.DebugLog("StopBot", DebugLogTypes.TwitchClipBot, "Stopping bot.");

                        ClipMonitorService.Stop();
                        IsActive = false;
                        InvokeBotStopped();
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "StopBot");
                    IsActive = false;
                    InvokeBotStopped();
                }
            });
        }

        public override Task<bool> ExitBot()
        {
            return Task.Run(() =>
            {
                if (IsActive == true)
                {
                    LogWriter.DebugLog("ExitBot", DebugLogTypes.TwitchClipBot, "Now stopping and exiting bot.");

                    StopBot();
                }
                return true;
            });
        }

        public async Task<List<Clip>> GetAllClipsAsync()
        {
            LogWriter.DebugLog("GetAllClipsAsync", DebugLogTypes.TwitchClipBot, "Getting all clips.");

            return await ClipMonitorService.GetAllClipsAsync(OptionFlags.TwitchStreamerUserId);
        }

        public async Task CreateClipAsync()
        {
            if (IsActive == true)
            {
                LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, "Creating a new clip.");

                // if create clip fails due to token, the token event will re-call this method
                ClipMonitorService?.CreateClip(OptionFlags.TwitchStreamerUserId);
            }
        }
    }
}

#endif
