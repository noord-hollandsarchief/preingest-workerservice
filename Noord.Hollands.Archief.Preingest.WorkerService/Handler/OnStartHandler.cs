﻿using System;
using System.Linq;

using Microsoft.Extensions.Logging;

using Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Handler
{

    /// <summary>
    /// Helper handler for starting the background service
    /// </summary>
    public static class OnStartHandler
    {
        /// <summary>
        /// Options for execution results
        /// </summary>
        private enum PreingestActionResults
        {
            None = 0,
            Executing = 1,
            Failed = 2,
            Error = 3,
            Success = 4
        }
        /// <summary>
        /// Options for an preingest action state
        /// </summary>
        private enum ContainerStatus
        {
            Success = 4,
            Failed = 3,
            Error = 2,
            Running = 1,
            New = 0,
            None = -1
        }

        /// <summary>
        /// Called when [start calculate].
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="next">The next.</param>
        /// <param name="plans">The plans.</param>
        /// <param name="actions">The actions.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static bool OnStartCalculate(Plan previous, Plan next, Plan[] plans, PreingestAction[] actions, ILogger<PreingestEventHubHandler> logger)
        {
            bool onStartError = false;
            
            if (next != null && plans != null && actions != null)
            {
                var currentScheduledPlanThatsDoneList = plans.Where(item => item.Status == ExecutionStatus.Done).ToArray();
                var currentScheduledPlanWithActionList = actions.Join(currentScheduledPlanThatsDoneList,
                    a => a.Name,
                    p => p.ActionName.ToString(),
                    (a, p) => a).OrderBy(item
                        => ((ContainerStatus)Enum.Parse(typeof(ContainerStatus), item.ActionStatus))).ThenBy(item
                            => item.Creation).ToList();

                logger.LogInformation("OnStartHandler :: Previous action : {0}.", previous == null ? "None" : previous.ActionName);
                logger.LogInformation("OnStartHandler :: Next action : {0}.", next == null ? "None" : next.ActionName);
                logger.LogInformation("OnStartHandler :: Current plan : {0}.", plans == null ? "None" : String.Join(",", plans.Select(item => item.ActionName).ToArray()));
                               
                if (currentScheduledPlanWithActionList.Count == 0)
                    return onStartError = true;

                var highestStatusResult = currentScheduledPlanWithActionList.First();

                var statusEnum = ((ContainerStatus)Enum.Parse(typeof(ContainerStatus), highestStatusResult.ActionStatus));
                
                logger.LogInformation("OnStartHandler :: Highest status result found {0} ({1}).", statusEnum, highestStatusResult.Name);
                //calculate current schedule plan action overall status....                        
                switch (statusEnum)
                {
                    //compare with configuration 
                    //determine result
                    case ContainerStatus.Success:
                        onStartError = true;
                        logger.LogInformation("OnStartHandler :: (Success) = true");
                        break;
                    case ContainerStatus.Error:
                        onStartError = next.StartOnError;
                        logger.LogInformation("OnStartHandler :: (Error) = {0}", next.StartOnError);
                        break;
                    case ContainerStatus.Failed:
                        onStartError = false;
                        logger.LogInformation("OnStartHandler :: (Failed) = false");
                        break;
                    case ContainerStatus.Running:
                    case ContainerStatus.New:
                    case ContainerStatus.None:
                    default:
                        onStartError = false;
                        logger.LogInformation("OnStartHandler :: (Default) = false");
                        break;
                }
            }
            return onStartError;
        }
    }
}
