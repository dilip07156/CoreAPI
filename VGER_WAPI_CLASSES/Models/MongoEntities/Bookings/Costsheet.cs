using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace VGER_WAPI_CLASSES
{
    public class Costsheet
    {
        public string QuotationPrice_Id { get; set; }
        public string Currency_Id { get; set; }
        public string Currency { get; set; }
        public string Version { get; set; }
        public bool? FOCDilution { get; set; }
        public string Markup_Id { get; set; }
        public string MarkupDetail_Id { get; set; }
        public string PercMarkup { get; set; }
        public string ExchangeRate_Id { get; set; } // This is exchange rate against base currency
        public float? ExchangeRate { get; set; }
        public string ExchangeCurrency_Id { get; set; }
        public string ExchangeCurrency { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public bool? AutoManual { get; set; }
        public bool? DetailedFOC { get; set; }

        public AuditTrail AuditTrail { get; set; }

    }
}
