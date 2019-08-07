using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI_CLASSES
{
    public class mGuesstimate
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string QRFID { get; set; }
        public string GuesstimateId { get; set; }
        public int VersionId { get; set; }
        public string VersionName { get; set; }
        public string VersionDescription { get; set; }
        public bool IsCurrentVersion { get; set; }
        public string ChangeRule { get; set; } = "PS"; //By default 'Preferred Supplier'
        public double? ChangeRulePercent { get; set; }
        public string CalculateFor { get; set; } = "ALL";
        public List<GuesstimatePosition> GuesstimatePosition { get; set; } = new List<GuesstimatePosition>();

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }

    public class GuesstimatePosition
    {
        public string GuesstimatePositionId { get; set; }
        public string PositionId { get; set; }
        public string ProductId { get; set; }
        public string DefaultSupplierId { get; set; }
        public string DefaultSupplier { get; set; }
        public string ActiveSupplierId { get; set; }
        public string ActiveSupplier { get; set; }
        public string Day { get; set; }
        public string PlaceOfService { get; set; }
        public DateTime? OriginalItineraryDate { get; set; }
        public string OriginalItineraryDay { get; set; }
        public string ProductCategory { get; set; }
        public string ProductCategoryId { get; set; }
        public bool KeepZero { get; set; }
        public string KeepAs { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ProductType { get; set; }
        public string OriginalItineraryDescription { get; set; }
        public string BuyCurrency { get; set; }
        public string ProductTypeChargeBasis { get; set; }
        public bool? StandardPrice { get; set; }
        public List<GuesstimatePrice> GuesstimatePrice { get; set; } = new List<GuesstimatePrice>();
    }

    public class GuesstimatePrice
    {
        public string GuesstimatePriceId { get; set; }
        public string PositionPriceId { get; set; }
        public string PositionId { get; set; }
        public long DepartureId { get; set; }
        public DateTime? Period { get; set; }
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
        public string ProductType { get; set; }
        public string KeepAs { get; set; }

        public string BuyCurrencyId { get; set; }
        public string BuyCurrency { get; set; }
        public string ContractId { get; set; }
        public double ContractPrice { get; set; }
        public double BudgetPrice { get; set; }
        public double BuyPrice { get; set; }
        public double MarkupAmount { get; set; }
        public double BuyNetPrice { get; set; }

        public string SellCurrencyId { get; set; }
        public string SellCurrency { get; set; }
        public double SellNetPrice { get; set; }
        public double TaxAmount { get; set; }
        public double SellPrice { get; set; }

        public string ExchangeRateId { get; set; }
        public double ExchangeRatio { get; set; }

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}
