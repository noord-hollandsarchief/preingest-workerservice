using Noord.Hollands.Archief.Preingest.WorkerService.Handler;
using Noord.Hollands.Archief.Preingest.WorkerService.Handler.Command;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey;

using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

using Microsoft.Extensions.Logging;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler.Creator
{
    /// <summary>
    /// Concrete class for command factory pattern for executing an API call
    /// </summary>
    /// <seealso cref="Noord.Hollands.Archief.Preingest.WorkerService.Handler.Creator.CommandCreator" />
    public class PreingestCommandCreator : CommandCreator
    {
        private readonly IDictionary<IKey, IPreingestCommand> _executionCommand = null;
        private Uri _webapiUrl = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="PreingestCommandCreator"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="webapiUrl">The webapi URL.</param>
        public PreingestCommandCreator(ILogger<PreingestEventHubHandler> logger, Uri webapiUrl) : base(logger)
        {
            _webapiUrl = webapiUrl;
            _executionCommand = new Dictionary<IKey, IPreingestCommand>();

            _executionCommand.Add(new DefaultKey(ValidationActionType.ContainerChecksumHandler), new CalculateChecksumCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ExportingHandler), new DroidCsvExportingCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ReportingPdfHandler), new DroidPdfReportingCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ReportingDroidXmlHandler), new DroidXmlReportingCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ReportingPlanetsXmlHandler), new DroidPlanetsReportingCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ProfilesHandler), new DroidProfilingCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.EncodingHandler), new EncodingCheckCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.UnpackTarHandler), new ExpandArchiveCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.MetadataValidationHandler), new MetadataSchemaCheckCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.NamingValidationHandler), new NamingCheckCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.GreenListHandler), new NhaGreenlistCheckCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ExcelCreatorHandler), new PreingestExcelReportingCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ScanVirusValidationHandler), new ScanVirusCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.SidecarValidationHandler), new SidecarCheckCommand(logger, webapiUrl));

            _executionCommand.Add(new DefaultKey(ValidationActionType.PrewashHandler), new PrewashCommand(logger, webapiUrl));
            //new: step
            _executionCommand.Add(new DefaultKey(ValidationActionType.BuildOpexHandler), new BuildOpexCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.FilesChecksumHandler), new ChecksumCompareCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.PolishHandler), new PolishOpexCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.UploadBucketHandler), new Upload2BucketCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ShowBucketHandler), new ShowBucketCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ClearBucketHandler), new ClearBucketCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.IndexMetadataHandler), new IndexingMetadataExcelReportingCommand(logger, webapiUrl));
            //new: fiets
            _executionCommand.Add(new DefaultKey(ValidationActionType.PasswordDetectionHandler), new PasswordDetectionCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.ToPX2MDTOHandler), new ConvertToPX2MDTOCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.PronomPropsHandler), new UpdatePronumMDTOCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.FixityPropsHandler), new UpdateFixityMDTOCommand(logger, webapiUrl));
            _executionCommand.Add(new DefaultKey(ValidationActionType.RelationshipHandler), new UpdateRelationshipMDTOCommand(logger, webapiUrl));
        }

        /// <summary>
        /// Method to determine which call will be used.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public override IPreingestCommand FactoryMethod(Guid guid, dynamic data)
        {
            if (data == null)
            {
                Logger.LogInformation("FactoryMethod :: {1} : {0}.", "Incoming data is empty", guid);
                return null;
            }

            Plan[] plans = data.scheduledPlan == null ? null : JsonConvert.DeserializeObject<Plan[]>(data.scheduledPlan.ToString());
            PreingestAction[] actions = data.preingest == null ? null : JsonConvert.DeserializeObject<PreingestAction[]>(data.preingest.ToString());

            if (plans == null)
            {
                Logger.LogInformation("FactoryMethod :: {1} : {0}.", "No scheduled plan found, just exit", guid);
                return null;
            }

            Queue<Plan> queue = new Queue<Plan>(plans);

            Plan next = null;
            Plan previous = null;
            while (queue.Count > 0)
            {
                Plan item = queue.Peek();
                //found one running (should not), just break it
                if (item.Status == ExecutionStatus.Executing)
                    break;

                //found one done (previous), peek if null done else next
                if (item.Status == ExecutionStatus.Done)
                {
                    previous = queue.Dequeue();
                    Plan peek = queue.Count > 0 ? queue.Peek() : null;

                    if (peek == null) //done just exit
                    {
                        Logger.LogInformation("FactoryMethod :: {1} : {0}.", "Peek queue. See nothing. Probably done with the plan", guid);
                        return null;
                    }

                    if (peek.Status == ExecutionStatus.Done)
                        continue;

                    if (peek.Status == ExecutionStatus.Pending)
                    {
                        next = queue.Dequeue();
                        break;
                    }
                }
                //found one pending, just fire next
                if (item.Status == ExecutionStatus.Pending)
                {
                    next = queue.Dequeue();
                    break;
                }
            }

            if (next == null)
            {
                Logger.LogInformation("FactoryMethod :: {1} : {0}.", "Exit the factory method, no next task planned", guid);
                return null;
            }

            if (previous == null && next != null)
            {
                IKey key = new DefaultKey(next.ActionName);
                if (!this._executionCommand.ContainsKey(key))
                {
                    Logger.LogInformation("FactoryMethod :: {1} : No key found in dictionary with {0}.", key, guid);
                    return null;
                }
                bool isNextOverallStatusOk2Run = OnStartError(previous, next, plans, actions);
                if (!isNextOverallStatusOk2Run)
                {
                    Logger.LogInformation("FactoryMethod :: {1} : Overall status tell us not to continue {0}.", key, guid);
                    return new FailCommand(next.ActionName, Logger, _webapiUrl);
                }

                IPreingestCommand command = this._executionCommand[key];
                return command;
            }

            if (previous != null && next != null)
            {
                if (actions == null)
                {
                    Logger.LogInformation("FactoryMethod :: {0} : Plan described previous and next action, but there is no actions list returned. Hmmm....", guid);
                    return null;
                }

                var action = actions.Where(item => item.Name == previous.ActionName.ToString()).FirstOrDefault();
                if (action == null)
                {
                    Logger.LogInformation("FactoryMethod :: {1} : No action found in the list with the name {0}.", previous.ActionName.ToString(), guid);
                    return null;
                }

                IKey key = new DefaultKey(next.ActionName);
                if (!this._executionCommand.ContainsKey(key))
                {
                    Logger.LogInformation("FactoryMethod :: {1} : No key found in dictionary with {0}.", key, guid);
                    return null;
                }

                bool isPreviousOk2Run = false;
                switch (action.ActionStatus)
                {
                    case "Error":
                        if (previous.ContinueOnError)
                        {
                            isPreviousOk2Run = true;
                        }
                        break;
                    case "Failed":
                        if (previous.ContinueOnFailed)
                        {
                            isPreviousOk2Run = true;
                        }
                        break;
                    case "Success":
                        {
                            isPreviousOk2Run = true;
                        }
                        break;
                    default:
                        isPreviousOk2Run = false;
                        break;
                }

                bool isNextOverallStatusOk2Run = OnStartError(previous, next, plans, actions);
                if (isPreviousOk2Run)
                {
                    if (!isNextOverallStatusOk2Run)
                    {
                        Logger.LogInformation("FactoryMethod :: {1} : Overall status tell us not to continue {0}.", key, guid);
                        return new FailCommand(next.ActionName, Logger, _webapiUrl);
                    }
                    IPreingestCommand command = this._executionCommand[key];
                    return command;
                }
                Logger.LogInformation("FactoryMethod :: {1} : Not OK to run {0}.", key, guid);
            }
            return null;
        }

        /// <summary>
        /// Called when [start error].
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="next">The next.</param>
        /// <param name="plans">The plans.</param>
        /// <param name="actions">The actions.</param>
        /// <returns></returns>
        private bool OnStartError(Plan previous, Plan next, Plan[] plans, PreingestAction[] actions)
        {
            bool result = false;
            try
            {
                result = OnStartHandler.OnStartCalculate(previous, next, plans, actions, Logger);                 
            }
            catch (Exception e)
            {
                Logger.LogError(e, "OnStart calculation failed!");
                result = false;
            }
            finally { }

            return result;
        }
    }
}
