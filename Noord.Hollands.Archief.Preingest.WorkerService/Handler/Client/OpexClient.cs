using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Noord.Hollands.Archief.Preingest.WorkerService.Entities.Event;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;

namespace Noord.Hollands.Archief.Preingest.WorkerService.OpenAPI
{
    /// <summary>
    /// Partial class of auto-generated objects in Swagger OpenAPI
    /// </summary>
    public partial class OpexClient : Noord.Hollands.Archief.Preingest.WorkerService.OpenAPI.swaggerClient
    {
        public OpexClient(string baseUrl, System.Net.Http.HttpClient httpClient) : base(baseUrl, httpClient)
        {
        }

        public async Task PrepareRequestAsync(System.Net.Http.HttpClient httpClient, System.Net.Http.HttpRequestMessage request, StringBuilder urlBuilder)
        {
            await PrepareRequestAsync(httpClient, request, urlBuilder.ToString());
        }

        public async Task PrepareRequestAsync(System.Net.Http.HttpClient httpClient, System.Net.Http.HttpRequestMessage request, String urlBuilder)
        {
            await Task.Run(() =>
            {
                // do nothing 
            });
        }

        public async Task ProcessResponseAsync(System.Net.Http.HttpClient httpClient, System.Net.Http.HttpResponseMessage response, System.Threading.CancellationToken token)
        {
            await Task.Run(() =>
            {
                //do nothing
            });
        }
    }
}
