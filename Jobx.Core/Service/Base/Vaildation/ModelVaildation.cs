﻿using Jobx.Core.Service.Timer.Dto;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jobx.Core.Service.Base.Vaildation
{
    /// <summary>
    /// 这个验证在实体内部的验证通过后，才会执行
    /// </summary>
    public class ModelVaildation
    {
        /// <summary>
        /// 添加银行卡
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public static ValidationResult ScheduleEntity(object value, ValidationContext validationContext)
        {
            //如果value是Model的实体类型，则验证value中指定的数据类型。
            if (value is ScheduleEntity item)
            {
                //验证请求Url
                //string Url = @"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%$#_]*)?$";
                //if (!Regex.IsMatch(item.RequestUrl, Url))
                //{
                //    return new ValidationResult("请求Url格式错误", new[] { "RequestUrl" });
                //}
                switch (item.TriggerType)
                {
                    case Timer.Enum.TriggerTypeEnum.None:
                        break;
                    case Timer.Enum.TriggerTypeEnum.Simple:
                        if (item.IntervalSecond < 1) 
                        {
                            return new ValidationResult("请填写大于0的间隔时间", new[] { "IntervalSecond" });
                        }
                        if (item.RunTimes < 1)
                        {
                            return new ValidationResult("请填写大于0的执行次数", new[] { "RunTimes" });
                        }
                        break;
                    case Timer.Enum.TriggerTypeEnum.Cron:
                        //验证Cron是否填写
                        if (string.IsNullOrEmpty(item.Cron))
                        {
                            return new ValidationResult("Cron表达式必须填写", new[] { "Cron" });
                        }
                        //验证Cron表达式
                        if (!CronExpression.IsValidExpression(item.Cron))
                        {
                            return new ValidationResult("Cron表达式拼写错误", new[] { "Cron" });
                        }
                        break;
                    default:
                        break;
                }
            }
            //验证成功
            return ValidationResult.Success;
        }
    }
}
