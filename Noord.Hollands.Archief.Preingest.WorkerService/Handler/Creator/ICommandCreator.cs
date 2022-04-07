using System;
using System.Net.Http;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler.Creator
{
    /// <summary>
    /// Interface command creator (command factory pattern)
    /// </summary>
    public interface ICommandCreator
    {
        /// <summary>
        /// Method for inheritance.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public IPreingestCommand FactoryMethod(Guid guid, dynamic data);
    }
}
