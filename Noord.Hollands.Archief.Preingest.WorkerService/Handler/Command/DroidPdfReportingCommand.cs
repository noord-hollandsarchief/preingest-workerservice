using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;

using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler.Command
{
    /// <summary>
    /// Concrete class command for calling the client to create a DROID classification PDF report.
    /// </summary>
    /// <seealso cref="Noord.Hollands.Archief.Preingest.WorkerService.Handler.AbstractPreingestCommand" />
    public class DroidPdfReportingCommand : AbstractPreingestCommand
    {
        public override ValidationActionType ActionTypeName => ValidationActionType.ReportingPdfHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DroidPdfReportingCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="webapi">The webapi.</param>
        public DroidPdfReportingCommand(ILogger<PreingestEventHubHandler> logger, Uri webapi) : base(logger, webapi) { }

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
                OpenAPI.PreingestClient api = new OpenAPI.PreingestClient(WebApi.ToString(), client);
                api.DroidReportingPDFDroidXMLPlanetsXMLAsync(id, "pdf").GetAwaiter().GetResult();
            });
        }
    }
}
