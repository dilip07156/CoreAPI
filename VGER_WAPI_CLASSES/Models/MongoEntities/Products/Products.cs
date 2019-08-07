using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Products
    {
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string VoyagerProduct_Id { get; set; }
        public string ProductType_Id { get; set; }
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public bool? Placeholder { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string Corner { get; set; }
        public string PostCode { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyId { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public string SupplierTel { get; set; }
        public string SupplierFax { get; set; }
        public string SupplierWeb { get; set; }
        public string SupplierEmail { get; set; }
        public string Status { get; set; }
        public HotelAdditionalInfo HotelAdditionalInfo { get; set; }
        public List<ProductDescription> ProductDescription { get; set; }
        public List<HotelMealPlan> HotelMealPlan { get; set; }
        public List<ProductFacility> ProductFacilities { get; set; }        //ForProduct = 1
        public List<ProductResource> ProductResources { get; set; }
        public List<NearByCities> NearByCities { get; set; }
        public List<ProductSupplier> ProductSuppliers { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
        public List<ProductAttributes> ProductAttributes { get; set; }
        public List<ClosedOutDate> ClosedOutDates { get; set; }
        public List<InAndAround> InAndAround { get; set; }
        public List<TermsAndConditions> TermsAndConditions { get; set; }
		[BsonIgnoreIfNull(true)]
		public List<Mappings> Mappings { get; set; }
		public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }

    public class ProductDescription
    {
        public string DescType { get; set; }
        public string Description { get; set; }
        //public string Description { get; set; }
        //public string Internal { get; set; }
        //public string Customer { get; set; }
        //public string Itinerary { get; set; }
        //public string Quotation { get; set; }
        //public string FIT_Child_Policy { get; set; }
        //public string Group_Child_Policy { get; set; }
        //public string Credit_Card_Info { get; set; }
        //public string Area_Transportation_Info { get; set; }
        //public string Resturants_Info { get; set; }
        //public string Meeting_Rooms_Info { get; set; }
    }

    public class HotelAdditionalInfo
    {
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string AlternateServies_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }

        public string LocationId { get; set; }
        public string Location { get; set; }
        public string StarRatingId { get; set; }
        public string StarRating { get; set; }
        public string BdgPriceCategoryId { get; set; }
        public string BdgPriceCategory { get; set; }
        public string ChainId { get; set; }
        public string Chain { get; set; }
        public string HotelTypeId { get; set; }
        public string HotelType { get; set; }
        public int NoOfRooms { get; set; }
    }

    public class HotelMealPlan
    {
        public string HotelMealPlan_Id { get; set; }
        public string MealPlan_Id { get; set; }
        public string MealPlan { get; set; }
        public string Description { get; set; }
    }

    public class ProductFacility
    {
        public string ProductFacilityId { get; set; }
        public string FacilityId { get; set; }
        public string FacilityDesc { get; set; }
        public string Status { get; set; }
    }

    public class ProductResource
    {
        public string ProductResource_Id { get; set; }
        public string ResourceType_Id { get; set; }
        public string ResourceType { get; set; }
        public string Description { get; set; }
        public string ImageSRC { get; set; }
        public Int16? OrderNr { get; set; }
    }

    public class NearByCities
    {
        public string ProductNearByCities_ID { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string Location { get; set; }
    }

    public class ProductSupplier
    {
        public string ProductSupplier_Id { get; set; }
        public string Company_Id { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
        public string ContactVia { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public bool? IsPreferred { get; set; }
        public bool? IsDefault { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ActiveFrom { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ActiveTo { get; set; }
        public string Note { get; set; }
        public string Contact_Sales_Id { get; set; }
        public string Contact_Sales_Name { get; set; }
        public string Contact_Sales_Email { get; set; }
        public string Contact_Group_Id { get; set; }
        public string Contact_Group_Name { get; set; }
        public string Contact_Group_Email { get; set; }
        public string Contact_FIT_Id { get; set; }
        public string Contact_FIT_Name { get; set; }
        public string Contact_FIT_Email { get; set; }
        public string Contact_Finance_Id { get; set; }
        public string Contact_Finance_Name { get; set; }
        public string Contact_Finance_Email { get; set; }
        public string Contact_Emergency_Id { get; set; }
        public string Contact_Emergency_Name { get; set; }
        public string Contact_Emergency_Email { get; set; }
        public string Contact_Complaints_Id { get; set; }
        public string Contact_Complaints_Name { get; set; }
        public string Contact_Complaints_Email { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }

    public class ProductCategory
    {
        public string ProductCategory_Id { get; set; }
        public string DefProductCategory_Id { get; set; }
        public string ProductCategoryName { get; set; }
        public string ParentCategory_Id { get; set; }
        public string ParentCategoryName { get; set; }
        public bool? IsDefault { get; set; }
        public string Status { get; set; }
        public string GroupBreakfast { get; set; }
        public List<ProductFacility> RoomFacilities { get; set; }        //ForProductRange = 1
        public List<ProductRange> ProductRanges { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }

    public class ProductRange
    {
        public string ProductRange_Id { get; set; }
        public string ProductTemplate_Id { get; set; }
        public string ProductTemplateCode { get; set; }
        public string ProductTemplateName { get; set; }
        public string PersonType_Id { get; set; }
        public string PersonType { get; set; }
        public string Agemin { get; set; }
        public string Agemax { get; set; }
        public int? Quantity { get; set; }
        public bool? IsMarkup { get; set; }
        public bool? AdditionalYn { get; set; }
        public string ProductMenu { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }

    public class ClosedOutDate
    {
        public string ClosedOutDate_Id { get; set; }
        public string BusinessType { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public bool? IsSpecificDate { get; set; }
        public string Status { get; set; }
        
        /// <summary>
        /// WeekDays Pattern - sunday to saturday pattern e.g. 0111001
        /// </summary>
        public string DayPattern { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? FromDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ToDate { get; set; }
    }

    public class InAndAround
    {
        public string ProductLandmark_ID { get; set; }
        public string LandmarkTypeId { get; set; }
        public string LandmarkType { get; set; }
        public string LandmarkId { get; set; }
        public string LandmarkName { get; set; }
        public string Distance { get; set; }
        public string DistanceUnit { get; set; }
        public string Direction { get; set; }
        public string Status { get; set; }
    }

    public class TermsAndConditions
    {
        [BsonIgnoreIfNull(true)]
        public string ProductContract_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string PricePeriod_Id { get; set; }

        public string VoyagerTermsAndConditions_Id { get; set; }
        public string For_Company { get; set; }
        public string For_Product { get; set; }
        public string DocumentType { get; set; }
        public int? OrderNr { get; set; }
        public string BusinessType { get; set; }
        public string Tcs { get; set; }
        public string Section { get; set; }
        public string Status { get; set; }
    }
}
