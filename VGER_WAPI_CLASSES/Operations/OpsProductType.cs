using System;
using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class OpsProductTypeGetReq
    {
        public string BookingNumber { get; set; }
        public string ProductType { get; set; }
        public string PositionId { get; set; } 
    }

    public class OpsProdTypePositionGetReq
    {
        public string BookingNumber { get; set; }
        public string ProductType { get; set; } 
        public string DayName { get; set; }
        public string PositionId { get; set; }
    }

    public class OpsProductTypeGetRes
    {
        public OpsProductTypeGetRes()
        {
            OpsProductTypeCommonFields = new OpsProductTypeCommonFields();
            Position = new Positions();
            ResponseStatus = new ResponseStatus();
            DayList = new List<AttributeValues>();
            OpsProductTypeDetails = new OpsProductTypeDetails();
            ScheduleDetailsList = new List<ScheduleDetails>();
            SpecificDayItineraryDetails = new List<ItineraryDetails>();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public OpsProductTypeCommonFields OpsProductTypeCommonFields { get; set; }
        public OpsProductTypeDetails OpsProductTypeDetails { get; set; }
        public Positions Position { get; set; } 
        public List<AttributeValues> DayList { get; set; }        
        public List<ScheduleDetails> ScheduleDetailsList { get; set; }
        public List<ItineraryDetails> SpecificDayItineraryDetails { get; set; }
    }

    public class OpsProductTypeCommonFields
    {
        public string BookingId { get; set; }
        public string BookingNumber { get; set; }
        public string ProductType { get; set; }
        public string PositionId { get; set; }
        public string DayName { get; set; }
        public string ChargeBasis { get; set; }
        public bool IsRealSupplier { get; set; }
        public string SystemCompany_Id { get; set; }
    }

    public class OpsProductTypeDetails
    {
        public OpsProductTypeDetails()
        {
            ProductSRPDetails = new ProductSRPDetails();
            ResponseStatus = new ResponseStatus();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public ProductSRPDetails ProductSRPDetails { get; set; }
    }

    public class ScheduleDetails
    {
        public string PositionId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class OpsProdRangePersTypeGetReq
    {
        public string ProductId { get; set; }
        public string ProdCategory { get; set; }
        public string ProdRangeId { get; set; }
    }

    public class OpsProdRangePersTypeGetRes
    {
        public string PersonTypeId { get; set; }
        public string PersonType { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
