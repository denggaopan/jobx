using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobx.Core.Service.Timer.Dto
{
    /// <summary>
    /// Job任务结果
    /// </summary>
    public class HttpResultModel
    {
        /// <summary>
        /// 返回值编码
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 返回值说明
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 返回数据集 
        /// </summary>
        public object data { get; set; }
    }
}
