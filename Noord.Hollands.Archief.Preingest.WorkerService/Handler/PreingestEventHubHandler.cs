using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Newtonsoft.Json;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;

using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities;
using Noord.Hollands.Archief.Preingest.WorkerService.Handler.Creator;


namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler
{
    /// <summary>
    /// Handler for connecting preingest API using websocket
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class PreingestEventHubHandler : IDisposable
    {
        /// <summary>
        /// Internal entity for holding data
        /// </summary>
        internal class BlockItem
        {
            public Guid SessionId { get; set; }
            public String Data { get; set; }
        }
        private readonly object consumeLock = new object();

        private BlockingCollection<BlockItem> _internalCollection = null;
        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        private HubConnection Connection { get; set; }
        private Uri WebApiUrl { get; set; }
        /// <summary>
        /// Gets or sets the current logger.
        /// </summary>
        /// <value>
        /// The current logger.
        /// </value>
        protected ILogger<PreingestEventHubHandler> CurrentLogger { get; set; }
        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        /// <value>
        /// The creator.
        /// </value>
        private ICommandCreator Creator { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="PreingestEventHubHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="appSettings">The application settings.</param>
        public PreingestEventHubHandler(ILogger<PreingestEventHubHandler> logger, AppSettings appSettings)
        {
            WebApiUrl = new Uri(appSettings.WebApiUrl);
            Init(appSettings.EventHubUrl);
            CurrentLogger = logger;
            Creator = new PreingestCommandCreator(logger, WebApiUrl);

            _internalCollection = new BlockingCollection<BlockItem>(10);
        }

        /// <summary>
        /// Initializes the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        private void Init(String url)
        {
            if (String.IsNullOrEmpty(url))
                return;

            Connection = new HubConnectionBuilder()
              .WithUrl(url)
              .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10) })
              .Build();

            Connection.Closed += Closed;
            Connection.Reconnected += Reconnected;
            Connection.Reconnecting += Reconnecting;
            Connection.On<Guid, String>("SendNoticeToWorkerService", (guid, jsonData) => RunNext(guid, jsonData));

            //using (var dbContext = new WorkerServiceContext())
                //dbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Reconnecting if not connected.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private Task Reconnecting(Exception arg)
        {
            if (Connection == null)            
                CurrentLogger.LogInformation("Hub connection state is empty! Not initialised. Please check if the URL is correct.");            
            else            
                CurrentLogger.LogInformation("Hub connection state - Reconnecting - {0}", Connection.State);
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reconnected. If not try again.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private Task Reconnected(string arg)
        {
            if (Connection == null)            
                CurrentLogger.LogInformation("Hub connection state is empty! Not initialised. Please check if the URL is correct.");            
            else            
                CurrentLogger.LogInformation("Hub connection state - Reconnected - {0}", Connection.State);
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Connection closed.
        /// </summary>
        /// <param name="arg">The argument.</param>
        private async Task Closed(Exception arg)
        {
            if (Connection == null)            
                CurrentLogger.LogInformation("Hub connection state is empty! Not initialised. Please check if the URL is correct.");            
            else            
                CurrentLogger.LogInformation("Hub connection state - Closed - {0}", Connection.State);
            
            await Task.Delay(5000);
        }

        /// <summary>
        /// Run the next item.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="jsonData">The json data.</param>
        private void RunNext(Guid guid, string jsonData)
        {
            _internalCollection.Add(new BlockItem { SessionId = guid, Data = jsonData });
            CurrentLogger.LogInformation("Hub incoming message - {0}.", guid);
        }

        /// <summary>
        /// Connect (if not it will try re-connect) with Preingest websocket
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public async Task<bool> Connect(CancellationToken token)
        {
            if(Connection == null)
            {
                CurrentLogger.LogInformation("Hub connection is empty! Not initialised. Please check if the URL is correct.");
                return false;
            }

            while (true)
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                DateTime buildDate = LinkerHelper.GetLinkerTimestampUtc(assembly);

                CurrentLogger.LogInformation(String.Format("{0} version {1}. Build date and time {2}.", fvi.ProductName, fvi.ProductVersion, DateTimeOffset.FromFileTime(buildDate.ToFileTime())));
                CurrentLogger.LogInformation("Hub connection state - Method(Connect) - {0}", Connection.State);
                if (Connection.State == HubConnectionState.Connected)
                {
                    lock (consumeLock)
                    {
                        while (!_internalCollection.IsCompleted)
                        {
                            BlockItem item = _internalCollection.Take();

                            Task.Run(() =>
                            {
                                try
                                {
                                    dynamic data = JsonConvert.DeserializeObject<dynamic>(item.Data);
                                    IPreingestCommand command = Creator.FactoryMethod(item.SessionId, data);

                                    if (command != null)
                                    {
                                        Settings settings = data.settings == null ? null : JsonConvert.DeserializeObject<Settings>(data.settings.ToString());

                                        using (HttpClient client = new HttpClient())
                                        {
                                            if (settings == null)
                                                command.Execute(client, item.SessionId);
                                            else
                                                command.Execute(client, item.SessionId, settings);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    CurrentLogger.LogInformation("An exception occurred with SessionId {0}.", item.SessionId);
                                    CurrentLogger.LogError(e, e.Message);
                                }
                                finally { }
                            });
                        }
                    }
                    return true;
                }

                try
                {
                    await Connection.StartAsync(token);
                    CurrentLogger.LogInformation("Hub connection state - Method(Connect) after StartAsync - {0}", Connection.State);
                    return true;
                }
                catch when (token.IsCancellationRequested)
                {
                    return false;
                }
                catch
                {
                    // Failed to connect, trying again in 5000 ms.
                    await Task.Delay(5000);
                }
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            if(this.Connection != null)
            {
                var dispose = Connection.DisposeAsync();
                if (dispose.GetAwaiter().IsCompleted)
                {
                    GC.SuppressFinalize(Connection);
                    Connection = null;
                }
            }

            if (this._internalCollection != null)
                this._internalCollection.Dispose();
        }
    }
}
