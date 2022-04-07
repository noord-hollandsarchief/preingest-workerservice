using Microsoft.Extensions.Logging;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler.Command
{
    /// <summary>
    /// Concrete class command, default fallback command when all other commands fails
    /// </summary>
    /// <seealso cref="Noord.Hollands.Archief.Preingest.WorkerService.Handler.AbstractPreingestCommand" />
    public class FailCommand : AbstractPreingestCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailCommand"/> class.
        /// </summary>
        /// <param name="currentAction">The current action.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="webApiUrl">The web API URL.</param>
        public FailCommand(ValidationActionType currentAction, ILogger<PreingestEventHubHandler> logger, Uri webApiUrl) : base(logger, webApiUrl)
        {
            ActionTypeName = currentAction;
        }

        public override ValidationActionType ActionTypeName { get; }

        /// <summary>
        /// Executes the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="currentFolderSessionId">The current folder session identifier.</param>
        public override void Execute(HttpClient client, Guid currentFolderSessionId)
        {
            TryExecuteOrCatch(client, currentFolderSessionId, (id) =>
            {
                throw new ApplicationException(String.Format ("Fail command is executed! Action for {0} will go to failed status.", ActionTypeName));
            });
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
                throw new ApplicationException(String.Format("Fail command is executed! Action for {0} will go to failed status.", ActionTypeName));
            });
        }
    }
}
