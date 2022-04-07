using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler.Creator
{
    /// <summary>
    /// Abstrac class command creator (command factory pattern)
    /// </summary>
    /// <seealso cref="Noord.Hollands.Archief.Preingest.WorkerService.Handler.Creator.ICommandCreator" />
    public abstract class CommandCreator : ICommandCreator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCreator"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CommandCreator(ILogger<PreingestEventHubHandler> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILogger<PreingestEventHubHandler> Logger { get; set; }
        /// <summary>
        /// Method to implement.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public abstract IPreingestCommand FactoryMethod(Guid guid, dynamic data);
    }

}
