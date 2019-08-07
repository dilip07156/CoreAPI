using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NLog;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class LogFilter : ActionFilterAttribute
    {
        private static Logger _logger = LogManager.GetLogger("Trace");
        public DateTime RequestDatetime = DateTime.Now;
        //private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public LogFilter(ILoggerFactory loggerFactory)
        {
            //_logger = loggerFactory.CreateLogger("LogFilter");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            RequestDatetime = DateTime.Now;
            LogEventInfo logrequest = new LogEventInfo(NLog.LogLevel.Trace, "Trace", string.Empty);
            TraceLog nlog = new TraceLog();

            nlog.LogDate = RequestDatetime;

            var ctx = context.HttpContext.Request;
            if (ctx != null)
            {
                nlog.ClientIP = Convert.ToString(ctx.Host);
                nlog.Method = Convert.ToString(ctx.Method);
                nlog.URL = ctx.Path.Value;
                nlog.MessageType = "Request";
            }
            nlog.LogType = "Trace";
            nlog.Format = "JSON";
            nlog.TraceId = context.HttpContext.TraceIdentifier.ToString();
            nlog.Application = "VOYAGER_DISTRIBUTION";
            nlog.Action = context.ActionDescriptor.DisplayName;
            nlog.Parameter = Newtonsoft.Json.JsonConvert.SerializeObject(context.ActionArguments);
            nlog.Token = (ctx.Cookies["JWTToken"] == null) ? string.Empty : ctx.Cookies["JWTToken"].ToString();
            nlog.HostIp = context.HttpContext.Connection.LocalIpAddress.Address.ToString();

            logrequest.Message = Newtonsoft.Json.JsonConvert.SerializeObject(nlog);
            _logger.Log(logrequest);   


            //_logger.LogInformation("OnActionExecuting");
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            DateTime ResponseDatetime = DateTime.Now;
            //DateTime RequestDatetime = (DateTime)context.HttpContext.Request.Properties[context.ActionDescriptor.DisplayName];

            LogEventInfo logrequest = new LogEventInfo(NLog.LogLevel.Trace, "Trace", string.Empty);
            TraceLog nlog = new TraceLog();

            nlog.LogDate = ResponseDatetime;
            var ctx = context.HttpContext.Request;
            if (ctx != null)
            {
                nlog.ClientIP = Convert.ToString(ctx.Host);
                nlog.Method = Convert.ToString(ctx.Method);
                nlog.URL = ctx.Path.Value;
                nlog.MessageType = "Request";
                nlog.ContentLength = (ctx.ContentLength == null) ? "" : ctx.ContentLength.ToString();
            }
            nlog.LogType = "Trace";
            nlog.Format = "JSON";
            nlog.TraceId = context.HttpContext.TraceIdentifier.ToString();
            nlog.Application = "VOYAGER_DISTRIBUTION";
            nlog.Action = context.ActionDescriptor.DisplayName;
            nlog.Token = (ctx.Cookies["JWTToken"] == null) ? string.Empty : ctx.Cookies["JWTToken"].ToString();
            nlog.HostIp = context.HttpContext.Connection.LocalIpAddress.Address.ToString();
            nlog.Parameter = Newtonsoft.Json.JsonConvert.SerializeObject(((Microsoft.AspNetCore.Mvc.ObjectResult)context.Result).Value);
            nlog.StatusCode = ((Microsoft.AspNetCore.Mvc.ObjectResult)context.Result).StatusCode.ToString();

            nlog.ResponseTime = (ResponseDatetime - RequestDatetime).TotalMilliseconds;
            
            logrequest.Message = Newtonsoft.Json.JsonConvert.SerializeObject(nlog);
            _logger.Log(logrequest);

            //_logger.LogInformation("OnActionExecuted");
            base.OnActionExecuted(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //_logger.LogInformation("OnResultExecuting");
            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            //_logger.LogInformation("OnResultExecuted");
            base.OnResultExecuted(context);
        }
    }
}
