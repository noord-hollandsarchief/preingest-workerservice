using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler.Command
{
    /// <summary>
    /// Concrete class command for calling the client to build an OPEX structure.
    /// </summary>
    /// <seealso cref="Noord.Hollands.Archief.Preingest.WorkerService.Handler.AbstractPreingestCommand" />
    public class RevertDipCollectionCommand : AbstractPreingestCommand
    {
        public override ValidationActionType ActionTypeName => ValidationActionType.RevertCollectionHandler;
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildNonMetadataOpexCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="webapi">The webapi.</param>
        public RevertDipCollectionCommand(ILogger<PreingestEventHubHandler> logger, Uri webapi) : base(logger, webapi) { }

        /// <summary>
        /// Executes the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="currentFolderSessionId">The current folder session identifier.</param>
        public override void Execute(HttpClient client, Guid currentFolderSessionId)
        {
            Execute(client, currentFolderSessionId, null);
        }

        /// <summary>
        /// Executes the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="currentFolderSessionId">The current folder session identifier.</param>
        /// <param name="settings">The settings.</param>
        public override void Execute(HttpClient client, Guid currentFolderSessionId, Settings settings)
        {
            TryExecuteOrCatch(client, currentFolderSessionId, (id) =>
            {
                Logger.LogInformation("Command: {0}", this.GetType().Name);
                OpenAPI.OpexClient api = new OpenAPI.OpexClient(WebApi.ToString(), client);
                api.RevertCollectionAsync(id).GetAwaiter().GetResult();
            });
        }
    }
}
