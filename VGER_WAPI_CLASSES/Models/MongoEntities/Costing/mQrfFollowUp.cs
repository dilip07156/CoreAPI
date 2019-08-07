using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class mQrfFollowUp
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string QRFID { get; set; }
        public List<FollowUpItem> FollowUp { get; set; } = new List<FollowUpItem>();
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
