﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey
{
    public class DefaultKey : AbstractKey
    {
        public DefaultKey(ValidationActionType name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
