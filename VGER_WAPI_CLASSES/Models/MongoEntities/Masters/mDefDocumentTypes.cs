using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mDefDocumentTypes
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
        public string DocumentTypeId { get; set; }
        public bool Active { get; set; } = true;
        public int? Doctypeid { get; set; }
        public string DocumentType { get; set; }
        public string Name { get; set; }
        public bool? Permanent { get; set; }
        public bool? System { get; set; }
        public string Template { get; set; }
        public string Dddoc { get; set; }
        public string Title { get; set; }
        public bool? ForCustomer { get; set; }
        public bool? ForSupplier { get; set; }
        public bool? ForBooking { get; set; }
        public bool? ForPosition { get; set; }
        public bool? Webcust { get; set; }
        public bool? Logcust { get; set; }
        public bool? Uniquename { get; set; }
        public string Status { get; set; }
        public string EmailMessage { get; set; }
        public string DocIntroline { get; set; }
        public string DocFooter { get; set; }
        public bool? AutoDocument { get; set; }
        public bool? AutoDelivery { get; set; }
        public string Action { get; set; }
    }
}
