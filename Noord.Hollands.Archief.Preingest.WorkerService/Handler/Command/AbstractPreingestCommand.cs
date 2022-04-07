using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Noord.Hollands.Archief.Preingest.WorkerService.OpenAPIService;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;

using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler
{
    /// <summary>
    /// Abstract class command. Ready to extend. Implement the required methods and props.
    /// </summary>
    /// <seealso cref="Noord.Hollands.Archief.Preingest.WorkerService.Handler.IPreingestCommand" />
    public abstract class AbstractPreingestCommand : IPreingestCommand
    {
        /// <summary>
        /// Custom event notification entity
        /// </summary>
        /// <seealso cref="System.EventArgs" />
        internal class NotificationEvent : EventArgs
        {
            public enum State
            {
                Started, 
                CompletedOrFailed
            }

            public State Result { get; set; }
            public HttpClient Client { get; set; }
            public Guid Id { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractPreingestCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="webApiUrl">The web API URL.</param>
        public AbstractPreingestCommand(ILogger<PreingestEventHubHandler> logger, Uri webApiUrl)
        {
            Logger = logger;
            WebApi = webApiUrl;
        }

        /// <summary>
        /// Occurs when [notify event].
        /// </summary>
        private event EventHandler<NotificationEvent> NotifyEvent;

        /// <summary>
        /// Called when [notify].
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnNotify(NotificationEvent e)
        {
            EventHandler<NotificationEvent> handler = NotifyEvent;
            if (handler != null)
                handler(this, e);

            Notify(e.Client, e.Id, e.Result);
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILogger<PreingestEventHubHandler> Logger { get; set; }
        /// <summary>
        /// Gets or sets the web API.
        /// </summary>
        /// <value>
        /// The web API.
        /// </value>
        protected Uri WebApi { get; set; }

        /// <summary>
        /// Gets the name of the action type.
        /// </summary>
        /// <value>
        /// The name of the action type.
        /// </value>
        public abstract Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey.ValidationActionType ActionTypeName { get; }

        /// <summary>
        /// Tries the execute or catch.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="actionMethod">The action method.</param>
        protected void TryExecuteOrCatch(HttpClient client, Guid guid, Action<Guid> actionMethod)
        {
            if (actionMethod == null)
                return;

            var start = DateTime.Now;
            bool isExecuted = false;
            try
            {
                actionMethod(guid);
                isExecuted = true;
            }
            catch (Exception e)
            {
                isExecuted = false;
                Logger.LogError(e, e.Message);
                TryAndRegisterFailedState(client, guid, e);
            }
            finally
            {
                if (isExecuted)
                {
                    var end = DateTime.Now;
                    TimeSpan processTime = (TimeSpan)(end - start);
                }
            }
        }

        /// <summary>
        /// Tries the execute or catch.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="actionMethod">The action method.</param>
        protected void TryExecuteOrCatch(HttpClient client, Guid guid, Settings settings, Action<Guid, Settings> actionMethod)
        {
            if (actionMethod == null)
                return;

            var start = DateTime.Now;
            bool isExecuted = false;
            try
            {
                actionMethod(guid, settings);
                isExecuted = true;
            }
            catch (Exception e)
            {
                isExecuted = false;
                Logger.LogError(e, e.Message);
                TryAndRegisterFailedState(client, guid, e);
            }
            finally
            {
                if (isExecuted)
                {
                    var end = DateTime.Now;
                    TimeSpan processTime = (TimeSpan)(end - start);
                }
            }
        }

        /// <summary>
        /// Method to override.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="currentFolderSessionId">The current folder session identifier.</param>
        public abstract void Execute(HttpClient client, Guid currentFolderSessionId);
        /// <summary>
        /// Method to override.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="currentFolderSessionId">The current folder session identifier.</param>
        /// <param name="settings">The settings.</param>
        public abstract void Execute(HttpClient client, Guid currentFolderSessionId, Settings settings);

        /// <summary>
        /// Tries the state of the and register failed.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="parentExc">The parent exc.</param>
        /// <exception cref="System.ApplicationException"></exception>
        private void TryAndRegisterFailedState(HttpClient client, Guid id, Exception parentExc)
        {
            try
            {
                Entities.CommandKey.ValidationActionType currentActionType = this.ActionTypeName;
                OpenAPIService.OutputClient api = new OpenAPIService.OutputClient(WebApi.ToString(), client);

                api.ProcessResponse += (object sender, Entities.Event.CallEvents e) =>
                {
                    dynamic collection = JsonConvert.DeserializeObject<dynamic>(e.ResponseMessage);

                    PreingestAction[] actions = collection.preingest == null ? new PreingestAction[] { } : JsonConvert.DeserializeObject<PreingestAction[]>(collection.preingest.ToString());

                    StringBuilder textBuilder = new StringBuilder();
                    textBuilder.AppendLine(parentExc.Message);
                    textBuilder.AppendLine(parentExc.StackTrace);                    

                    var currentAction = actions.FirstOrDefault(action => action.Name == currentActionType.ToString());
                    if (currentAction == null)
                    {
                        CompleteNewActionRegistration(client, id, textBuilder.ToString());
                    }
                    else if (currentAction.States == null)
                    {
                        CompleteNewStatesRegistration(client, id, currentAction.ProcessId, textBuilder.ToString());
                    }
                    else if (currentAction.States != null && currentAction.States.Length > 0)
                    {
                        if (currentAction.States.Length >= 2)
                            throw new ApplicationException(String.Format("Cannot register fault state! Action ID {0} have already 2 items in states collection. Folder/Session ID {1}", currentAction.ProcessId, id));

                        FailedStateRegistration(client, id, currentAction.ProcessId, textBuilder.ToString());
                    }                    
                };

                var response = api.CollectionAsync(id).GetAwaiter().GetResult();
                if (response.StatusCode != 200)
                    throw new ApplicationException(String.Format("Failed to register fault state for action {0} with ID {1}! Web API status code returned not 200 code.", currentActionType, id));                                
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
            }
            finally { }
        }

        /// <summary>
        /// Completes the new action registration.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="folderSessionId">The folder session identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="System.ApplicationException">
        /// Parsing process (action) ID failed!
        /// or
        /// </exception>
        private void CompleteNewActionRegistration(HttpClient client, Guid folderSessionId, string errorMessage)
        {
            Entities.CommandKey.ValidationActionType currentActionType = this.ActionTypeName;

            OpenAPIService.StatusClient status = new OpenAPIService.StatusClient(WebApi.ToString(), client);
            //add new 
            Guid processId = Guid.Empty;
            status.ProcessResponse += (object sender, Entities.Event.CallEvents e) =>
            {
                dynamic actionRegistration = JsonConvert.DeserializeObject<dynamic>(e.ResponseMessage);
                object id = actionRegistration.processId;
                bool isParsed = Guid.TryParse(id == null ? "" : id.ToString(), out processId);
            };

            var addNewActionResponse = status.NewAsync(folderSessionId, new BodyNewAction
            {
                Name = currentActionType.ToString(),
                Description = "Action created by WorkerService.",
                Result = null
            }).GetAwaiter().GetResult();

            if (processId == Guid.Empty)
                throw new ApplicationException("Parsing process (action) ID failed!");

            if (addNewActionResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("New action registration returned a bad response (not 200 code)! Action ID {0}, Folder/Session ID {1}.", processId, folderSessionId));
            //add 2 records (started/failed) for state
            var startStateResponse = status.StartAsync(processId).GetAwaiter().GetResult();           
            if (startStateResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("Start state registration returned a bad response (not 200 code)! Action ID {0}, Folder / Session ID {1}.", processId, folderSessionId));
            
            OnNotify(new NotificationEvent { Client = client, Id = folderSessionId, Result = NotificationEvent.State.Started });
            
            var failedStateResponse = status.FailedAsync(processId, new BodyMessage { Message = errorMessage }).GetAwaiter().GetResult();
            if (failedStateResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("Failed state registration returned a bad response (not 200 code)! Action ID {0}, Folder / Session ID {1}.", processId, folderSessionId));

            SummaryItem summaryObj = new SummaryItem { Accepted = 0, Processed = 0, Rejected = 1, Start = DateTimeOffset.Now, End = DateTimeOffset.Now };
            string summaryStr = JsonConvert.SerializeObject(summaryObj);
            //final update
            var finalUpdateResponse = status.UpdateAsync(processId, new BodyUpdate { Result = "Failed", Summary = summaryStr }).GetAwaiter().GetResult();
            if(finalUpdateResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("Final update status returned a bad response (not 200 code)! Action ID {0}, Folder / Session ID {1}.", processId, folderSessionId));

            OnNotify(new NotificationEvent { Client = client, Id = folderSessionId, Result = NotificationEvent.State.CompletedOrFailed });
        }

        /// <summary>
        /// Completes the new states registration.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="folderSessionId">The folder session identifier.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="System.ApplicationException"></exception>
        private void CompleteNewStatesRegistration(HttpClient client, Guid folderSessionId, Guid processId, string errorMessage)
        {
            Entities.CommandKey.ValidationActionType currentActionType = this.ActionTypeName;

            OpenAPIService.StatusClient status = new OpenAPIService.StatusClient(WebApi.ToString(), client);

            //add 2 records (started/failed) for state
            var startStateResponse = status.StartAsync(processId).GetAwaiter().GetResult();
            if (startStateResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("Start state registration returned a bad response (not 200 code)! Action ID {0}, Folder / Session ID {1}.", processId, folderSessionId));

            OnNotify(new NotificationEvent { Client = client, Id = folderSessionId, Result = NotificationEvent.State.Started });

            var failedStateResponse = status.FailedAsync(processId, new BodyMessage { Message = errorMessage }).GetAwaiter().GetResult();
            if (failedStateResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("Failed state registration returned a bad response (not 200 code)! Action ID {0}, Folder / Session ID {1}.", processId, folderSessionId));

            SummaryItem summaryObj = new SummaryItem { Accepted = 0, Processed = 0, Rejected = 1, Start = DateTimeOffset.Now, End = DateTimeOffset.Now };
            string summaryStr = JsonConvert.SerializeObject(summaryObj);
            //final update
            var finalUpdateResponse = status.UpdateAsync(processId, new BodyUpdate { Result = "Failed", Summary = summaryStr }).GetAwaiter().GetResult();
            if (finalUpdateResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("Final update status returned a bad response (not 200 code)! Action ID {0}, Folder / Session ID {1}.", processId, folderSessionId));

            OnNotify(new NotificationEvent { Client = client, Id = folderSessionId, Result = NotificationEvent.State.CompletedOrFailed });
        }

        /// <summary>
        /// Faileds the state registration.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="folderSessionId">The folder session identifier.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="System.ApplicationException"></exception>
        private void FailedStateRegistration(HttpClient client, Guid folderSessionId, Guid processId, string errorMessage)
        {
            OpenAPIService.StatusClient status = new OpenAPIService.StatusClient(WebApi.ToString(), client);

            var failedStateResponse = status.FailedAsync(processId, new BodyMessage { Message = errorMessage }).GetAwaiter().GetResult();
            if (failedStateResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("Failed state registration returned a bad response (not 200 code)! Action ID {0}, Folder / Session ID {1}.", processId, folderSessionId));

            SummaryItem summaryObj = new SummaryItem { Accepted = 0, Processed = 0, Rejected = 1, Start = DateTimeOffset.Now, End = DateTimeOffset.Now };
            string summaryStr = JsonConvert.SerializeObject(summaryObj);
            //final update
            var finalUpdateResponse = status.UpdateAsync(processId, new BodyUpdate { Result = "Failed", Summary = summaryStr }).GetAwaiter().GetResult();
            if (finalUpdateResponse.StatusCode != 200)
                throw new ApplicationException(String.Format("Final update status returned a bad response (not 200 code)! Action ID {0}, Folder / Session ID {1}.", processId, folderSessionId));

            OnNotify(new NotificationEvent { Client = client, Id = folderSessionId, Result = NotificationEvent.State.CompletedOrFailed });
        }

        /// <summary>
        /// Notifies the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="folderSessionId">The folder session identifier.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ApplicationException"></exception>
        private void Notify(HttpClient client, Guid folderSessionId, NotificationEvent.State type)
        {
            Entities.CommandKey.ValidationActionType currentActionType = this.ActionTypeName;

            OpenAPIService.StatusClient status = new OpenAPIService.StatusClient(WebApi.ToString(), client);

            if (type == NotificationEvent.State.Started)
            {
                //notify start
                var startBody = new BodyEventMessageBody
                {
                    Accepted = 0,
                    Processed = 0,
                    Rejected = 0,
                    Name = currentActionType.ToString(),
                    HasSummary = true,
                    SessionId = folderSessionId,
                    State = "Started",
                    EventDateTime = DateTimeOffset.Now,
                    Start = DateTimeOffset.Now,
                    End = DateTimeOffset.Now,
                    Message = "Event triggered by WorkerService."
                };
                var statusStartResult = status.NotifyAsync(startBody).GetAwaiter().GetResult();
                if (statusStartResult.StatusCode != 200)
                    throw new ApplicationException(String.Format("Failed to notify, returned a bad response (not 200 code)! Folder / Session ID {0}.", folderSessionId));             
            }

            if (type == NotificationEvent.State.CompletedOrFailed)
            {
                //notify end
                var failedBody = new BodyEventMessageBody
                {
                    Accepted = 0,
                    Processed = 0,
                    Rejected = 0,
                    Name = currentActionType.ToString(),
                    HasSummary = true,
                    SessionId = folderSessionId,
                    State = "Failed",
                    EventDateTime = DateTimeOffset.Now,
                    Start = DateTimeOffset.Now,
                    End = DateTimeOffset.Now,
                    Message = "Event triggered by WorkerService."
                };
                var statusFailedResult = status.NotifyAsync(failedBody).GetAwaiter().GetResult();
                if (statusFailedResult.StatusCode != 200)
                    throw new ApplicationException(String.Format("Failed to notify, returned a bad response (not 200 code)! Folder / Session ID {0}.", folderSessionId));
            }

            System.Threading.Thread.Sleep(500);
        }          
    }
}
