using Jobx.Core.Const;
using Jobx.Core.Dapper;
using Jobx.Core.Service.App.Dto;
using Jobx.Core.Service.App.Enum;
using Jobx.Core.Service.Timer.Dto;
using Jobx.Core.Service.Timer.Enum;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Talk.Extensions;
using Talk.Extensions.Helper;
using Flurl.Http;
using Jobx.Common.DingTalkRobot.Robot;
using System.Text.RegularExpressions;

namespace Jobx.Core.Service.Timer
{
    public class HttpJob : JobBase<LogUrlModel>, IJob
    {
        private IFlurlResponse flurlResponse;

        public HttpJob() : base(new LogUrlModel())
        { }

        public override async Task NextExecute(IJobExecutionContext context)
        {
            //获取相关参数
            var requestUrl = context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTURL)?.Trim();
            requestUrl = requestUrl?.IndexOf("http") == 0 ? requestUrl : "http://" + requestUrl;
            var requestParameters = context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTPARAMETERS);
            requestParameters = string.IsNullOrEmpty(requestParameters) ? "{}" : requestParameters;
            var headersString = context.JobDetail.JobDataMap.GetString(QuartzConstant.HEADERS);
            var headers = headersString != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(headersString?.Trim()) : null;
            var requestType = (RequestTypeEnum)int.Parse(context.JobDetail.JobDataMap.GetString(QuartzConstant.REQUESTTYPE));
            var TimeOut = 30;
            if (!string.IsNullOrEmpty(context.JobDetail.JobDataMap.GetString(QuartzConstant.TIMEOUT)))
            {
                TimeOut = context.JobDetail.JobDataMap.GetIntValueFromString(QuartzConstant.TIMEOUT);
            }
            var CovenantReturnModel = false;
            if (!string.IsNullOrEmpty(context.JobDetail.JobDataMap.GetString(QuartzConstant.CovenantReturnModel))) 
            {
                CovenantReturnModel = context.JobDetail.JobDataMap.GetBooleanValueFromString(QuartzConstant.CovenantReturnModel);
            }

            LogInfo.Url = requestUrl;
            LogInfo.RequestType = requestType.ToString();
            LogInfo.Parameters = requestParameters;

            HttpResponseMessage response = new HttpResponseMessage();
            var http = HttpHelper.Instance;
            LogInfo.Req_Url = requestUrl;
            LogInfo.Req_Type = LogInfo.RequestType;
            LogInfo.Headers = headersString;
            LogInfo.Result = requestParameters;

            var content = new StringContent(requestParameters, Encoding.UTF8, "application/json");
            switch (requestType)
            {
                case RequestTypeEnum.Get:
                    //response = await http.GetAsync(requestUrl, headers);
                    if (headers != null) 
                    {
                        flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(TimeOut).GetAsync();
                    }
                    else 
                    {
                        flurlResponse = await requestUrl.WithTimeout(TimeOut).GetAsync();
                    }
                    response = flurlResponse.ResponseMessage;
                    break;
                case RequestTypeEnum.Post:
                    if (headers != null)
                    {
                        flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(TimeOut).PostAsync(content);
                    }
                    else
                    {
                        flurlResponse = await requestUrl.WithTimeout(TimeOut).PostAsync(content);
                    }
                    response = flurlResponse.ResponseMessage;
                    break;
                case RequestTypeEnum.Put:
                    if (headers != null)
                    {
                        flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(TimeOut).PutAsync(content);                        
                    }
                    else
                    {
                        flurlResponse = await requestUrl.WithTimeout(TimeOut).PutAsync(content);                        
                    }
                    response = flurlResponse.ResponseMessage;
                    break;
                case RequestTypeEnum.Delete:
                    if (headers != null)
                    {
                        flurlResponse = await requestUrl.WithHeaders(headers).WithTimeout(TimeOut).DeleteAsync();
                    }
                    else
                    {
                        flurlResponse = await requestUrl.WithTimeout(TimeOut).DeleteAsync();
                    }
                    response = flurlResponse.ResponseMessage;
                    break;
            }
            var result = HttpUtility.HtmlEncode(Regex.Unescape(await response.Content.ReadAsStringAsync()));
            LogInfo.Req_Data = requestParameters;
            LogInfo.Result = HttpUtility.HtmlDecode(result);

            if (!response.IsSuccessStatusCode)
            {
                LogInfo.Status = ExecutionStatusEnum.Failure;
                LogInfo.ErrorMsg = $"<span class='error'>{result.MaxLeft(3000)}</span>";
                await ErrorAsync(LogInfo.JobName, new Exception(result.MaxLeft(3000)), JsonConvert.SerializeObject(LogInfo));
                context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
            }
            else
            {
                try
                {
                    if (CovenantReturnModel)
                    {
                        //这里需要和请求方约定好返回结果约定为HttpResultModel模型
                        var httpResult = JsonConvert.DeserializeObject<HttpResultModel>(HttpUtility.HtmlDecode(result));
                        if ( httpResult.code > 0)
                        {
                            LogInfo.Status = ExecutionStatusEnum.Failure;
                            LogInfo.ErrorMsg = $"<span class='error'>{httpResult.message}</span>";
                            await ErrorAsync(LogInfo.JobName, new Exception(httpResult.message), JsonConvert.SerializeObject(LogInfo));
                            context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
                        }
                        else
                        {
                            LogInfo.Status = ExecutionStatusEnum.Success;
                        }
                    }
                    else 
                    {
                        LogInfo.Status = ExecutionStatusEnum.Success;
                    }
                }
                catch (Exception ex)
                {
                    if (CovenantReturnModel && ex.Message.Contains("Unexpected character encountered while parsing value")) 
                    {
                        var httpResult = HttpUtility.HtmlDecode(result);
                        LogInfo.Status = ExecutionStatusEnum.Failure;
                        await ErrorAsync(LogInfo.JobName, new Exception($"未按约定模型返回响应数据,当前响应数据:{httpResult}"), JsonConvert.SerializeObject(LogInfo));
                        throw new Exception($"未按约定模型返回响应数据,当前响应数据:{httpResult}");
                    }
                    LogInfo.Status = ExecutionStatusEnum.Failure;
                    await ErrorAsync(LogInfo.JobName, ex, JsonConvert.SerializeObject(LogInfo));
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
