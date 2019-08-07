using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mStatus
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string Status_Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public bool ForCompany { get; set; }
        public bool ForBooking { get; set; }
        public bool ForPosition { get; set; }
        public bool ForDocument { get; set; }
        public bool ForProduct { get; set; }
        public bool ForContact { get; set; }
        public bool ForDiary { get; set; }
        public bool ForAgentExtranet { get; set; }
        public bool ForSupplierExtranet { get; set; }
        public bool ForCustomerExtranet { get; set; }

    }
}
