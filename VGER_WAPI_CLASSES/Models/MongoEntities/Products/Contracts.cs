using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Mongo Object for Contract Collection on its whole
    /// </summary>
    public class Contracts
    {
        /// <summary>
        /// SQL Column: ProductContract_Id
        /// </summary>
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string ProductContract_Id { get; set; }

        /// <summary>
        /// SQL Column: ProductSupplier_Id
        /// </summary>
        public string ProductSupplier_Id { get; set; }
        /// <summary>
        /// SQL Column: Status
        /// </summary>
        public string Status { get; set; }

        // Supplier Info
        public string Supplier_Id { get; set; }
        public string Supplier_Code { get; set; }
        public string Supplier_Name { get; set; }
        // Supplier Info

        // Product Info
        public string Product_Id { get; set; }
        public string Product_Code { get; set; }
        public string Product_Name { get; set; }
        public string Product_Type_Id { get; set; }
        public string Product_Type { get; set; }
        public string Country_Id { get; set; }
        public string Country_Name { get; set; }
        public string City_Id { get; set; }
        public string City_Name { get; set; }
        // Product Info

        /// <summary>
        /// SQL Column: BusinessType
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// SQL Column: FromDate
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Contract_Start_Date { get; set; }
        /// <summary>
        /// SQL Column: EndDate
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Contract_End_Date { get; set; }

        /// <summary>
        /// SQL Column: Currency_Id
        /// </summary>
        public string Contract_Currency_Id { get; set; }
        public string Contract_Currency { get; set; }

        /// <summary>
        /// SQL Column: ContractType
        /// </summary>
        public string Contract_Type { get; set; }
        public string Contract_Type_FullName { get; set; }
        /// <summary>
        /// SQL Column: IsContractLocked
        /// </summary>
        public bool IsLocked { get; set; }
        /// <summary>
        /// SQL Column: BuySellContractType
        /// </summary>
        public string BuySellType { get; set; }
        /// <summary>
        /// SQL Column: CopiedContract_Id
        /// </summary>
        public string ParentContract_Id { get; set; }
        /// <summary>
        /// SQL Column: BuyContract_Id
        /// </summary>
        public string BuyContract_Id { get; set; }
        /// <summary>
        /// SQL Column: Markups_Id
        /// </summary>
        public string SellMarkup_Id { get; set; }
        public string SellMarkup { get; set; }

        /// <summary>
        /// SQL Column: Agent_Id
        /// </summary>
        public string ForAgent_Id { get; set; }
        public string ForAgent_Code { get; set; }
        public string ForAgent_Name { get; set; }
        /// <summary>
        /// SQL Column: Market_Id
        /// </summary>
        public string ForMarket_Id { get; set; }
        public string ForMarket_Name { get; set; }
        public List<ContractBusinessRegions> BusinessRegions { get; set; }

        /// <summary>
        /// SQL Column: SignedBy_Id
        /// </summary>
        public string DMC_Signatory_Id { get; set; }
        public string DMC_Signatory_Name { get; set; }
        public string DMC_Signatory_Email { get; set; }
        /// <summary>
        /// SQL Column: SignedBySupp_Id
        /// </summary>
        public string Supplier_Signatory_Id { get; set; }
        public string Supplier_Signatory_Name { get; set; }
        public string Supplier_Signatory_Email { get; set; }

        public List<PricePeriods> PricePeriods { get; set; }
        public List<TermsAndConditions> Terms { get; set; }
        public List<ProductContractFOC> FOC { get; set; }
        public AuditTrail AuditTrail { get; set; }
    }

    public class ContractBusinessRegions
    {
        [BsonIgnoreIfNull(true)]
        public string ProductContract_Id { get; set; }

        public string BusinessRegion_Id { get; set; }
        public string BusinessRegion_Name { get; set; }
    }

    public class PricePeriods
    {
        [BsonIgnoreIfNull(true)]
        public string ProductContract_Id { get; set; }

        /// <summary>
        /// SQL Column: PricePeriod_Id
        /// </summary>
        public string PricePeriod_Id { get; set; }
        /// <summary>
        /// SQL Column: PricePeriodName
        /// </summary>
        public string PricePeriod_Name { get; set; }

        /// <summary>
        /// SQL Column: BUSITYPES
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// SQL Column: DATEMIN
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Period_Start_Date { get; set; }
        /// <summary>
        /// SQL Column: DATEMAX
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Period_End_Date { get; set; }
        /// <summary>
        /// SQL Column: CXDEADLINE
        /// </summary>
        public int? Cancellation_Deadline { get; set; }

        /// <summary>
        /// SQL Column: DayCombo_Id
        /// </summary>
        public string DayCombo_Id { get; set; }
        public string DayCombo { get; set; }
        /// <summary>
        /// SQL Column: PricePeriod_Type_ID
        /// </summary>
        public string PricePeriod_Type_ID { get; set; }
        public string PricePeriod_Type { get; set; }
        /// <summary>
        /// SQL Column: EXCEPTION
        /// </summary>
        public string RateType_Code { get; set; }
        public string RateType_Name { get; set; }

        public int? Min_Nights { get; set; }
        public int? Min_Quantity { get; set; }
        public int? Max_Quantity { get; set; }

        /// <summary>
        /// SQL Column: Company_Id
        /// </summary>
        public string ForAgent_Id { get; set; }
        public string ForAgent_Code { get; set; }
        public string ForAgent_Name { get; set; }

        /// <summary>
        /// SQL Column: BuyPricePeriod_ID
        /// </summary>
        public string BuyPricePeriod_ID { get; set; }

        public List<TermsAndConditions> Terms { get; set; }
        public List<ContractPrices> Prices { get; set; }
    }

    public class ContractPrices
    { 
        [BsonIgnoreIfNull(true)]
        public string PricePeriod_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string ProductContract_Id { get; set; }

        /// <summary>
        /// SQL Column: ProductPrice_ID
        /// </summary>
        public string ProductPrice_ID { get; set; }

        /// <summary>
        /// SQL Column: ProductRange_ID
        /// </summary>
        public string ProductRange_ID { get; set; }
        public string ProductTemplate_ID { get; set; }
        public string ProductTemplate { get; set; }
        public string ProductTemplate_Full { get; set; }
        public string PersonType_ID { get; set; }
        public string PersonType { get; set; }
        public int? Age_Min { get; set; }
        public int? Age_Max { get; set; }
        /// <summary>
        /// SQL Column: ChargeBasis
        /// </summary>
        public string ChargeBasis { get; set; }

        /// <summary>
        /// SQL Column: MealPlan_Id
        /// </summary>
        public string MealPlan_Id { get; set; }
        public string MealPlan { get; set; }
        /// <summary>
        /// SQL Column: OptionalYN
        /// </summary>
        public bool? IsOptional { get; set; }
        /// <summary>
        /// SQL Column: InclusiveTaxYN
        /// </summary>
        public bool? InclusiveTaxYN { get; set; }

        /// <summary>
        /// SQL Column: PRICE
        /// </summary>
        public decimal? Contract_Price { get; set; }
        /// <summary>
        /// SQL Column: CURRENCY
        /// </summary>
        public string CURRENCY { get; set; }
        /// <summary>
        /// SQL Column: NetNetPrice
        /// </summary>
        public decimal? Contract_NetNetPrice { get; set; }

        /// <summary>
        /// SQL Column: BuyProductPrice_Id
        /// </summary>
        public string BuyProductPrice_Id { get; set; }
        public string Status { get; set; }
        public AuditTrail AuditTrail { get; set; }
    }

    public class ProductContractFOC
    {
        [BsonIgnoreIfNull(true)]
        public string ProductContract_Id { get; set; } 

        /// <summary>
        /// SQL Column: ProductFreePlacePolicy_Id
        /// </summary>
        public string ProductFreePlacePolicy_Id { get; set; }

        /// <summary>
        /// SQL Column: DATEMIN
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime FromDate { get; set; }
        /// <summary>
        /// SQL Column: DATEMAX
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ToDate { get; set; }

        /// <summary>
        /// SQL Column: ProductTemplate_ID
        /// </summary>
        public string ProductTemplate_ID { get; set; }
        public string ProductTemplate { get; set; }
        public string ProductTemplate_Full { get; set; }

        /// <summary>
        /// SQL Column: MINPERS
        /// </summary>
        public decimal? MINPERS { get; set; }
        /// <summary>
        /// SQL Column: QUANTITY
        /// </summary>
        public decimal? QUANTITY { get; set; }

        public AuditTrail AuditTrail { get; set; }
    }

    public class ProductContractInfoReq
    {
        public string ProductContract_Id { get; set; }
        public string PricePeriod_Id { get; set; }
    }

    public class PricePeriodInfoReq
    {
        public string ProductContract_Id { get; set; }
        public List<string> PricePeriodsList { get; set; }
    }
}
