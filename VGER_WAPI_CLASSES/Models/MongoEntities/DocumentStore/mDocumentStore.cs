using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mDocumentStore
    {
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string Document_Id { get; set; }

        public string DocumentType { get; set; }

        public string QRFID { get; set; }
        [BsonIgnoreIfNull(true)]
        public string QRFPriceId { get; set; }
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string AlternateServiceId { get; set; }
        public string SupplierId { get; set; }
        public string ClientId { get; set; }
        public string SystemCompany_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string VoyagerUser_Id { get; set; }

		public string From { get; set; }
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public List<string> BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<string> DocumentPath { get; set; }
        public string Importance { get; set; }
        public DateTime? SendDate { get; set; }
        public string SendStatus { get; set; }
        [BsonIgnoreIfNull(true)]
        public string ErrorMessage { get; set; }

        //[BsonIgnoreIfNull(true)]
        //public List<string> ErrorMessages { get; set; }

        [BsonIgnoreIfNull(true)]
        public string MailStatus { get; set; }         
        public DateTime? RetryDate { get; set; }

        public string Send_Via { get; set; }
        public string DocumentReference { get; set; }
        public DateTime? DocumentSigned { get; set; }
        public string SupplierConfNum { get; set; }
        public string DocumentSignInUser { get; set; }

        public DateTime Create_Date { get; set; }
        public string Create_User { get; set; }
        public DateTime? Edit_Date { get; set; }
        public string Edit_User { get; set; } 
    }
}