using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Entities
{
    /// <summary>
    /// Application settings for the workerservice
    /// </summary>
    public class AppSettings
    {
        public String EventHubUrl { get; set; }
        public String WebApiUrl { get; set; }
    }
}
