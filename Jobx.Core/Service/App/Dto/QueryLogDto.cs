﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobx.Core.Service.App.Dto
{
    public class QueryLogDto
    {
        public string group { get; set; }

        public string name { get; set; }

        public int? status { get; set; }

        public string start_time { get; set; }

        public string end_time { get; set; }
    }
}
