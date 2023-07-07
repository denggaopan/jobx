using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobx.Core.Entity
{
    public class JOB_DETAILS
    {
        public string JOB_NAME { get; set; }

        public string JOB_GROUP { get; set; }

        public string DESCRIPTION { get; set; }

        public byte[] JOB_DATA { get; set; }
    }
}
