using Noord.Hollands.Archief.Preingest.WorkerService.Entities.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noord.Hollands.Archief.Preingest.WorkerService.OpenAPI
{

    /// <summary>
    /// Partial class of auto-generated objects in Swagger OpenAPI
    /// </summary>
    public partial class StatusClient : Noord.Hollands.Archief.Preingest.WorkerService.OpenAPI.swaggerClient
    {
        public StatusClient(string baseUrl, System.Net.Http.HttpClient httpClient) : base(baseUrl, httpClient)
        {
        }

        public event EventHandler<CallEvents> ProcessResponse;

        protected virtual void OnTrigger(CallEvents e)
        {
            EventHandler<CallEvents> handler = ProcessResponse;
            if (handler != null)
                handler(this, e);
        }

        public async Task PrepareRequestAsync(System.Net.Http.HttpClient httpClient, System.Net.Http.HttpRequestMessage request, StringBuilder urlBuilder)
        {
            await PrepareRequestAsync(httpClient, request, urlBuilder.ToString());
        }

        public async Task PrepareRequestAsync(System.Net.Http.HttpClient httpClient, System.Net.Http.HttpRequestMessage request, String urlBuilder)
        {
            await Task.Run(() =>
            {
                //do nothing
            });
        }

        public async Task ProcessResponseAsync(System.Net.Http.HttpClient httpClient, System.Net.Http.HttpResponseMessage response, System.Threading.CancellationToken token)
        {
            OnTrigger(new CallEvents { ResponseMessage = await response.Content.ReadAsStringAsync() });
        }
    }
}
