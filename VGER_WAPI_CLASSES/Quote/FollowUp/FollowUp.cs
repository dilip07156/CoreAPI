using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class FollowUp
    {
        [BsonId]
        public string FollowUp_Id { get; set; }
        public List<FollowUpTask> FollowUpTask { get; set; } = new List<FollowUpTask>();
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }

    public class FollowUpTask
    {
        public string Task { get; set; }
        public string FollowUpType { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? FollowUpDateTime { get; set; }
        public string FromContact_Id { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string ToContact_Id { get; set; }
        public string ToName { get; set; }
        public string ToEmail { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public string FollowUpStatus { get; set; }
    }
}
