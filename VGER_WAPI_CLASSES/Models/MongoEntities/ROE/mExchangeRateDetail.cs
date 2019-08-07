using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mExchangeRateDetail
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string ExchangeRateDetail_Id { get; set; }
        public string ExchangeRate_Id { get; set; }
        public string Currency_Id { get; set; }
        public string CURRENCY { get; set; }
        public decimal RATE { get; set; }
        public decimal ROUNDTO { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }
}
