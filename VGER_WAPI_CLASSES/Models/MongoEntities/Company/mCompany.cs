using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mCompanies
    {
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string Company_Id { get; set; }
        public string Name { get; set; }
        public string CompanyCode { get; set; }
        public string SystemCompany_Id { get; set; }
        public bool? Iscustomer { get; set; }
        public bool? Issupplier { get; set; }
        public bool? Issubagent { get; set; }
        //public string IsAgent { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string Street3 { get; set; }
        public string Zipcode { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string Country_Id { get; set; }
        public string City_Id { get; set; }
        public string ParentAgent_Id { get; set; }
        public string ParentAgent_Name { get; set; }
        public string DefaultMarkup_Id { get; set; }
        public string B2b2bmarkup_Id { get; set; }
        // public decimal? GroupProfitPerc { get; set; }
        public string LogoFilePath { get; set; }
        public string HeadOffice_Id { get; set; }
        public string HeadOffice_Name { get; set; }
        public bool? AutoDelivery { get; set; }
        public bool? AutoGenerate { get; set; }
        public bool? VATAPPLICABLE { get; set; }

        public bool? B2C { get; set; }
        public bool? ISUSER { get; set; }
        public string ContactBy { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<TaxRegestrationDetails> TaxRegestrationDetails { get; set; } = new List<TaxRegestrationDetails>();

        [BsonIgnoreIfNull(true)]
        public List<CompanyMarkup> Markups { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<Targets> Targets { get; set; }

        public List<CompanyAccounts> AccountDetails { get; set; } = new List<CompanyAccounts>();
        public List<CompanyContacts> ContactDetails { get; set; } = new List<CompanyContacts>();
        public List<CompanyTerms> TermsAndConditions { get; set; } = new List<CompanyTerms>();
        public List<CompanyResources> Resources { get; set; } = new List<CompanyResources>();
        [BsonIgnoreIfNull(true)]
        public List<ChildrenCompanies> SalesOffices { get; set; }
        public List<ChildrenCompanies> Branches { get; set; } = new List<ChildrenCompanies>();
        public List<PaymentTerms> PaymentTerms { get; set; } = new List<PaymentTerms>();
        public List<EmergencyContacts> EmergencyContacts { get; set; } = new List<EmergencyContacts>();
        public List<PaymentDetails> PaymentDetails { get; set; } = new List<PaymentDetails>();
        [BsonIgnoreIfNull(true)]
        public List<CompanyProducts> Products { get; set; }
        [BsonIgnoreIfNull(true)]
        public List<Mappings> Mappings { get; set; }

        public string STATUS { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } 
    }

    public class TaxRegestrationDetails
    {
        public string TaxReg_Id { get; set; }
        public string Type { get; set; }
        public string State_Id { get; set; }
        public string State { get; set; }
        public string Number { get; set; }
        public string TaxStatus { get; set; }
        public string Status { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
        public string EditUser { get; set; }
        public string Type_Id { get; set; }
        public string Taxstatus_Id { get; set; }
        public string Company_id { get; set; }
        

    }
    public class PaymentDetails
    {
        public string PaymentDetails_Id { get; set; }
        public string Company_Id { get; set; }
        public string Details { get; set; }
        public string Currency_Id { get; set; }
        public string Currency { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
    }

    public class PaymentTerms
    {
        public string PaymentTerms_Id { get; set; }
        public string Company_Id { get; set; }
        public string From { get; set; }
        public int? Days { get; set; }
        public string ValueType { get; set; }
        public double? Value { get; set; }
        public string Currency_Id { get; set; }
        public string Currency { get; set; }
        public bool? VoucherReleased { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Crea_Dt { get; set; }
        public string Crea_Us { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Modi_Dt { get; set; }
        public string Modi_Us { get; set; }
        public string STATUS { get; set; }
        public string BusiType { get; set; }
    }

    public class ChildrenCompanies
    {
        public string ParentCompany_Id { get; set; }
        public string Company_Id { get; set; }
        public string Company_Code { get; set; }
        public string Company_Name { get; set; }
    }

    public class CompanyMarkup
    {
        public string VoyagerCompanyMarkup_Id { get; set; }
        public string Markup_For { get; set; }
        public string Markup_Basis { get; set; }
        public string Markup_Id { get; set; }
        public string Markup { get; set; }
        public decimal? Markup_Value { get; set; }
    }

    public class CompanyAccounts
    {
        public string VoyagerCompany_Id { get; set; }
        public string Name { get; set; }
        public string VATNumber { get; set; }
        public string FinanceContact { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public string SortCode { get; set; }
        public string IBAN { get; set; }
        public string Swift { get; set; }
        public string FinanceNote { get; set; }
    }

    public class CompanyTerms
    {
        public string TermsAndConditions_Id { get; set; }
        public string Company_Id { get; set; }
        public string DocumentType { get; set; }
        public int? OrderNr { get; set; }
        public string BusinessType { get; set; }
        public string Status { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string TermsDescription { get; set; }
        public string CreateUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
    }

    public class CompanyResources
    {
        public string CompanyResources_Id { get; set; }
        public string ResourcesType_Id { get; set; }
        public string Company_Id { get; set; }
        public string ResourcesType { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public int? OrderNr { get; set; }
        public string FilePath { get; set; }
        public string CreateUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
    }

    public class CompanyProducts
    {
        public string ProductSupplier_Id { get; set; }
        public string Supplier_Id { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string Product_Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ProductStatus { get; set; }
        public string SupplierStatus { get; set; }
        public string ContactVia { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public bool? IsPreferred { get; set; }
        public bool? IsDefault { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public string Note { get; set; }
        public string Contact_Sales_Id { get; set; }
        public string Contact_Sales_Name { get; set; }
        public string Contact_Sales_Email { get; set; }
        public string Contact_FIT_Id { get; set; }
        public string Contact_FIT_Name { get; set; }
        public string Contact_FIT_Email { get; set; }
        public string Contact_Group_Id { get; set; }
        public string Contact_Group_Name { get; set; }
        public string Contact_Group_Email { get; set; }
        public string Contact_Finance_Id { get; set; }
        public string Contact_Finance_Name { get; set; }
        public string Contact_Finance_Email { get; set; }
        public string Contact_Emergency_Id { get; set; }
        public string Contact_Emergency_Name { get; set; }
        public string Contact_Emergency_Email { get; set; }
        public string Contact_Complaints_Id { get; set; }
        public string Contact_Complaints_Name { get; set; }
        public string Contact_Complaints_Email { get; set; }
        public List<ProductSupplierSalesMarket> SalesMarket { get; set; } = new List<ProductSupplierSalesMarket>();
        public List<ProductSupplierOperatingMarket> OperatingMarket { get; set; } = new List<ProductSupplierOperatingMarket>();
        public List<ProductSupplierSalesAgent> SalesAgent { get; set; } = new List<ProductSupplierSalesAgent>();
        public string CreateUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; } = null;
    }

    public class ProductSupplierSalesMarket
    {
        public string ProductSupplierSalesMkt_Id { get; set; }
        public string BusinessRegion_Id { get; set; }
        public string BusinessRegion { get; set; }
    }

    public class ProductSupplierOperatingMarket
    {
        public string ProductSupplierOperatingMkt_Id { get; set; }
        public string BusinessRegion_Id { get; set; }
        public string BusinessRegion { get; set; }
    }

    public class ProductSupplierSalesAgent
    {
        public string ProductSupplierSalesAgent_Id { get; set; }
        public string Company_Id { get; set; }
        public string Company_Code { get; set; }
        public string Company_Name { get; set; }
    }

    public class mCompany
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerCompany_Id { get; set; }
        public string Name { get; set; }
        public string SystemCompany_Id { get; set; }
        public string CompanyCode { get; set; }
        public string CountryName { get; set; }
        public bool? Iscustomer { get; set; }
        public bool? Issupplier { get; set; }
        public bool? Issubagent { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string Street3 { get; set; }
        public string Postbox { get; set; }
        public string Zippostbox { get; set; }
        public string Zipcode { get; set; }
        public string CityName { get; set; }
        public string Region { get; set; }
        public string Countryid { get; set; }
        public string DefaultMarkup_Id { get; set; }
        public string FinanceContact_Id { get; set; }
        public string CityId { get; set; }
        public string DefaultFitmarkup_Id { get; set; }
        public string DefaultSeriesMarkup_Id { get; set; }
        public string B2b2bmarkup_Id { get; set; }
        public string ParentAgent_Id { get; set; }
        public decimal? GroupProfitPerc { get; set; }
        public decimal? FitprofitPerc { get; set; }
        public decimal? SeriesProfitPerc { get; set; }
        public string B2b2markUpBasis { get; set; }
        public decimal? B2b2markUpValue { get; set; }
        public string SeriesMarkUpBasis { get; set; }
        public decimal? SeriesMarkUpValue { get; set; }
        public string FitMarkUpBasis { get; set; }
        public decimal? FitMarkUpValue { get; set; }
        public string GrpMarkUpBasis { get; set; }
        public decimal? GrpMarkUpValue { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }

    public class CompanyAccountsAndMarkup
    {
        public string Company_Id { get; set; }
        public List<CompanyMarkup> Markups { get; set; }
        public CompanyAccounts CompanyAccounts { get; set; }
    }

    public class Mappings
    {
        public string Application_Id { get; set; }
        public string Application { get; set; }
        public string PartnerEntityCode { get; set; }
        public string PartnerEntityName { get; set; }
        public string PartnerEntityType { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public string AdditionalInfoType { get; set; }
        public string AdditionalInfo { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }

    public class Targets
    {
        public string TargetId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Month { get; set; }
        public int QRFs { get; set; }
        public int Bookings { get; set; }
        public int Passengers { get; set; }
        public decimal Income { get; set; }
        public string CreateUser { get; set; } 
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; } 
        public DateTime? EditDate { get; set; }
    }
}