﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub
{
    public class ActionStatusItem
    {
        public Guid StatusId { get; set; }
        public String Name { get; set; }
        public DateTimeOffset Creation { get; set; }
    }
}
