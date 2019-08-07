using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class FollowUpItem
    {
        public string FollowUp_Id { get; set; }
        public string Task { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? InternalFollowUpDateTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ExternalFollowUpDateTime { get; set; }
        public string InternalName { get; set; }
        public string ExternalName { get; set; }
        public string Notes { get; set; }
        public string InternalStatus { get; set; }
        public string ExternalStatus { get; set; }
        public string FollowUpStatus { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
