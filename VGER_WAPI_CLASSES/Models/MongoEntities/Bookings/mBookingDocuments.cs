using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mBookingDocuments
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string BookingDocumentId { get; set; }

        public string BookingId { get; set; }

        public string BookingNumber { get; set; }

        public string DocumentId { get; set; }

        public string Type { get; set; }

        public string FilePath { get; set; }

        public string CreateUser { get; set; } = "";

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}
