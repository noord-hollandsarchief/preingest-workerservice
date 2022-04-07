﻿using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;

using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler.Command
{
    /// <summary>
    /// Concrete class command for calling the client to do a fixity calcution and check for a TAR collection file.
    /// </summary>
    /// <seealso cref="Noord.Hollands.Archief.Preingest.WorkerService.Handler.AbstractPreingestCommand" />
    public class CalculateChecksumCommand : AbstractPreingestCommand
    {
        public override ValidationActionType ActionTypeName => ValidationActionType.ContainerChecksumHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateChecksumCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="webapi">The webapi.</param>
        public CalculateChecksumCommand(ILogger<PreingestEventHubHandler> logger, Uri webapi) : base(logger, webapi) { }

        /// <summary>
        /// Executes the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="currentFolderSessionId">The current folder session identifier.</param>
        /// <param name="settings">The settings.</param>
        public override void Execute(HttpClient client, Guid currentFolderSessionId, Settings settings)
        {
            TryExecuteOrCatch(client, currentFolderSessionId, settings, (id, props) =>
            {                
                Logger.LogInformation("Command: {0}", this.GetType().Name);
                OpenAPIService.PreingestClient api = new OpenAPIService.PreingestClient(WebApi.ToString(), client);

                if (props == null)
                {            
                    string message = "Settings is empty! Object reference is null. Please save the settings first before starting the run.";                 
                    throw new ApplicationException(message);
                }

                api.CalculateAsync(id, new OpenAPIService.BodyChecksum
                {
                    ChecksumType = props.ChecksumType,
                    InputChecksumValue = props.ChecksumValue
                }).GetAwaiter().GetResult();                
            });
        }

        /// <summary>
        /// Executes the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="currentFolderSessionId">The current folder session identifier.</param>
        public override void Execute(HttpClient client, Guid currentFolderSessionId)
        {
            Execute(client, currentFolderSessionId, null);
        }
    }
}
