using MongoDB.Bson.Serialization.Attributes;
using System;

namespace VGER_WAPI_CLASSES
{
    public class MongoPushRefeshRes
    {
        public string MongoPushRefesh_Master_Id { get; set; }
        public string MongoPushRefesh_Log_Id { get; set; }
    }

    public class MongoPushRefreshReq
    {
        public string Entity { get; set; }
        public string Type { get; set; }
        public string MongoPushRefesh_LogId { get; set; }
        public bool? IsSuccess { get; set; } = null;
        public string TotalRecords { get; set; } 
        public string DelRecords { get; set; }
        public string UpsertRecords { get; set; } 
        public string ErrorMessage { get; set; }
        public string NoOfRecords { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CurrentDT { get; set; }
    } 
}
