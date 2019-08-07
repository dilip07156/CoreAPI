using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mExchangeRate
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string ExchangeRateId { get; set; }
        public string ExRate { get; set; }
        public string RefCur { get; set; }
        public DateTime? DateMin { get; set; }
        public DateTime? DateMax { get; set; }
        public decimal VatRate { get; set; }
        public String Currency_Id { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }
}
