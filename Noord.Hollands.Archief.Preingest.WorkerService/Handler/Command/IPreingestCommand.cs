using System;
using System.Net.Http;

using Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler
{
    /// <summary>
    /// Interface command factory pattern
    /// </summary>
    public interface IPreingestCommand
    {
        public ValidationActionType ActionTypeName { get; }
        public void Execute(HttpClient client, Guid currentFolderSessionId);
        public void Execute(HttpClient client, Guid currentFolderSessionId, Settings settings);
    }
}
