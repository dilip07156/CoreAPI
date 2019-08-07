using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class TraceLog
    {
        public DateTime LogDate { get; set; }
        public string ClientIP { get; set; }
        public string LogType { get; set; }
        public string Format { get; set; }
        public string Method { get; set; }
        public string URL { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string StatusCode { get; set; }
        public string Parameter { get; set; }
        public string MessageType { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string TraceId { get; set; }
        public string Application { get; set; }
        public string HostIp { get; set; }
        public int TotalRecords { get; set; }
        public double ResponseTime { get; set; }
        public string ContentLength { get; set; }

    }
}
