using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace VGER_WAPI_CLASSES
{
    public class mTermsAndConditions
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerTermsAndConditions_Id { get; set; }
        public string For_Company { get; set; }
        public string CompanyId { get; set; }
        public string For_Product { get; set; }
        public string ProductId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentTypeId { get; set; }
        public int? OrderNr { get; set; }
        public string BusinessType { get; set; }
        public string Tcs { get; set; }
        public string Section { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
    }
}
