using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SummaryDetailsInfo
    {       
        public string Day { get; set; }

        public string PlaceOfService { get; set; }

        public string CountryName { get; set; }

        public DateTime? OriginalItineraryDate { get; set; }

        public string OriginalItineraryDay { get; set; }

        public string OriginalItineraryName { get; set; }

        public string ToCityName { get; set; }

        public string ToCountryName { get; set; }

        public string RoutingCityIds { get; set; }

        public List<Meal> IncludedMeals { get; set; }

        //public List<HotelInfo> Hotel { get; set; }

        public List<OriginalItineraryDetailsInfo> OriginalItineraryDetails { get; set; }

        public SummaryDetailsInfo()
        {
            OriginalItineraryDetails = new List<OriginalItineraryDetailsInfo>();
            IncludedMeals = new List<Meal>();
            //Hotel = new List<HotelInfo>();
        }
    }

    public class OriginalItineraryDetailsInfo
    {
        public string PositionId { get; set; }

        public string TLRemarks { get; set; }

        public string OPSRemarks { get; set; }

        public string OriginalItineraryDescription { get; set; }

        public string SupplierId { get; set; }

        public string Supplier { get; set; }

        public string ProductId { get; set; }

        public string ProductCode { get; set; }

        public bool? Placeholder { get; set; }

        public string Allocation { get; set; }

        public int? NumberOfPax { get; set; }

        public string ProductType { get; set; }

        public bool IsDeleted { get; set; }

        public string KeepAs { get; set; }

        public string ProductCategoryId { get; set; }

        public string ProductCategory { get; set; }

        public string ProductTypeChargeBasis { get; set; }

        public string BuyCurrency { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string MealType { get; set; }

        public string CityName { get; set; }

        public string CountryName { get; set; }

        public string Stars { get; set; }

        public int Duration { get; set; }

        public string ProductDescription { get; set; }

        public bool? StandardPrice { get; set; }
        
        public List<RoomInfo> RoomDetails { get; set; }

        public OriginalItineraryDetailsInfo()
        {
            RoomDetails = new List<RoomInfo>();
        }
    }   

    public class RoomInfo
    {
        public string ProductRangeId { get; set; }

        public string ProductRange { get; set; }

        public string ProdDesc { get; set; }

        public string RangeDesc { get; set; }
    }
}
