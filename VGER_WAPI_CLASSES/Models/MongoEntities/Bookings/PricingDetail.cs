using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Mongo Object for Booking Collection on its whole
    /// </summary>
    public class Pricing
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }

        public string Pricing_Id { get; set; }

        public string Position_Id { get; set; }
        public string BuyCurrency_Id { get; set; }
        public string BuyCurrency_Name { get; set; }
        public string SellCurrency_Id { get; set; }
        public string SellCurrency_Name { get; set; }

        public string Status { get; set; }

        /// <summary>
        /// SQL Column : BudgetBuyingPrice
        /// </summary>
        public Decimal? BudgetPrice { get; set; }
        /// <summary>
        /// SQL Column : Adj_Amount
        /// </summary>
        public Decimal? BudgetSupplementPrice { get; set; }
        public Decimal? BuyingPrice { get; set; }
        /// <summary>
        /// SQL Column : MarkUp
        /// </summary>
        public Decimal? MarkupAmt { get; set; }
        public Decimal? Net_SellPrice { get; set; }
        /// <summary>
        /// SQL Column : Tax_Price
        /// </summary>
        public Decimal? Taxes { get; set; }
        public Decimal? Gross_SellPrice { get; set; }


        /// <summary>
        /// SQL Column : NetMargin
        /// </summary>
        public Decimal? Net_Margin { get; set; }
        /// <summary>
        /// SQL Column : GrossMargin
        /// </summary>
        public Decimal? Gross_Margin { get; set; }
        /// <summary>
        /// SQL Column : NetMarginPerc
        /// </summary>
        public Decimal? Net_Margin_Perc { get; set; }
        /// <summary>
        /// SQL Column : GrossMarginPerc
        /// </summary>
        public Decimal? Gross_Margin_Perc { get; set; }


        public Decimal? Variance_Perc { get; set; }
        public Decimal? Variance_Amt { get; set; }
        /// <summary>
        /// SQL Column : SELLEXCHANGERATIO
        /// </summary>
        public Decimal? Sell_ExchangeRatio { get; set; }
        /// <summary>
        /// SQL Column : Season_Id
        /// </summary>
        public string BookingSeason_Id { get; set; }

        public List<PricingDetail> PricingDetail { get; set; }

        public AuditTrail AuditTrail { get; set; }

        #region Nullable Fields
        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CREA_US { get; set; }

        [BsonIgnoreIfNull(true)]
        public string MODI_US { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STATUS_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string STATUS_US { get; set; }
        #endregion
    }

    public class PricingDetail
    {
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }

        public string PricingDetail_Id { get; set; }
        public string Pricing_Id { get; set; }
        public string BookingRooms_Id { get; set; }
        /// <summary>
        /// SQL Column : PositionPrice_Id
        /// </summary>
        public string PositionPricing_Id { get; set; }
        public string ProductRange_Id { get; set; }
        public string ProductRange_Name { get; set; }
        public int? Units { get; set; }

        /// <summary>
        /// SQL Column : StartDate
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? PriceDate { get; set; }

        public string BuyCurrency_Id { get; set; }
        public string BuyCurrency { get; set; }
        public string SellCurrency_Id { get; set; }
        public string SellCurrency { get; set; }

        public Decimal? UnitBudgetPrice { get; set; }
        /// <summary>
        /// SQL Column : BudgetBuyingPrice
        /// </summary>
        public Decimal? BudgetPrice { get; set; }
        /// <summary>
        /// SQL Column : Adj_Amount
        /// </summary>
        public Decimal? BudgetSupplementPrice { get; set; }
        public Decimal? BuyingPrice { get; set; }
        /// <summary>
        /// SQL Column : MarkUpPrice
        /// </summary>
        public Decimal? MarkupAmt { get; set; }
        public Decimal? Net_SellPrice { get; set; }
        /// <summary>
        /// SQL Column : TaxPrice
        /// </summary>
        public Decimal? Taxes { get; set; }
        public Decimal? Gross_SellPrice { get; set; }


        /// <summary>
        /// SQL Column : NetMargin
        /// </summary>
        public Decimal? Net_Margin { get; set; }
        /// <summary>
        /// SQL Column : GrossMargin
        /// </summary>
        public Decimal? Gross_Margin { get; set; }
        /// <summary>
        /// SQL Column : NetMarginPerc
        /// </summary>
        public Decimal? Net_Margin_Perc { get; set; }
        /// <summary>
        /// SQL Column : GrossMarginPerc
        /// </summary>
        public Decimal? Gross_Margin_Perc { get; set; }


        public Decimal? Variance_Perc { get; set; }
        public Decimal? Variance_Amt { get; set; }
        public string MarkupDetail_Id { get; set; }
        public string ExchangeRateDetail_Id { get; set; }
        /// <summary>
        /// SQL Column : SELLEXCHANGERATIO
        /// </summary>
        public Decimal? Sell_ExchangeRatio { get; set; }

        /// <summary>
        /// SQL Column : Season_Id
        /// </summary>
        public string BookingSeason_Id { get; set; }
        public bool? IsInvoiced { get; set; }

        public AuditTrail AuditTrail { get; set; }

        #region Nullable Fields
        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CREA_US { get; set; }

        [BsonIgnoreIfNull(true)]
        public string MODI_US { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STATUS_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string STATUS_US { get; set; }
        #endregion
    }
}
