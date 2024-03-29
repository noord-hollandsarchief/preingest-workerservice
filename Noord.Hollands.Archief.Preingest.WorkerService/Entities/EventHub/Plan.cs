﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecutionStatus
    {
        Done = 0,
        Pending = 1,
        Executing = 2
    }

    public class Plan
    {
        public ExecutionStatus Status { get; set; }
        public ValidationActionType ActionName { get; set; }
        public bool ContinueOnError { get; set; }
        public bool ContinueOnFailed { get; set; }
        public bool StartOnError { get; set; }        
    }
}
