using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace VGER_WAPI_CLASSES
{
    public class mPositionPrice
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        //Unique Id for mPositionPrice
        public string PositionPriceId { get; set; }

        /// <summary>
        /// Take all following properties from mQuote and mPosition collections
        /// </summary>
        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public long DepartureId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Period { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ContractPeriod { get; set; }
        public long PaxSlabId { get; set; }
        public string PaxSlab { get; set; }
        public string Type { get; set; }
        public string RoomId { get; set; }
        public bool IsSupplement { get; set; }
        public string SupplierId { get; set; }
        public string Supplier { get; set; }
        public string ProductCategoryId { get; set; }
        public string ProductCategory { get; set; }        
        public string ProductRangeId { get; set; }
        public string ProductRange { get; set; }
        public string ProductRangeCode { get; set; }
        [BsonIgnoreIfNull(true)]
        public string TourEntityId { get; set; }

        /// <summary>
        /// Following properties for Buying
        /// </summary>
        public string BuyCurrencyId { get; set; }
        public string BuyCurrency { get; set; }
        public string ContractId { get; set; }
        public double ContractPrice { get; set; }
        public double BudgetPrice { get; set; }
        public double BuyPrice { get; set; }
        public double MarkupAmount { get; set; }
        public double BuyNetPrice { get; set; }

        /// <summary>
        /// Following properties for Selling
        /// </summary>
        public string SellCurrencyId { get; set; }
        public string SellCurrency { get; set; }
        public double SellNetPrice { get; set; }
        public double TaxAmount { get; set; }
        //gross price or sell price with tax
        public double SellPrice { get; set; }

        /// <summary>
        /// Following properties from ExchangeRateDetail collection
        /// </summary>
        public string ExchangeRateId { get; set; }
        public double ExchangeRatio { get; set; }

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}
