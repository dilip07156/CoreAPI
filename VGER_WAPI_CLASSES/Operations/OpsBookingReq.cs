using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VGER_WAPI_CLASSES
{
    public class OpsBookingSetReq
    {
        public string BookingNumber { get; set; }
        public List<string> PositionIds { get; set; }
        public string UserEmailId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<OpsKeyValue> OpsKeyValue;
        public string ModuleParent { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string DocType { get; set; }
        public bool IsSendEmail { get; set; }
        public bool IsSaveDocStore { get; set; }
        public OpsBookingSetReq()
        {
            OpsKeyValue = new List<OpsKeyValue>();
        }
    }

    public class OpsBookingSetRes
    {
        public OpsBookingSetRes()
        {
            ResponseStatus = new OPSWorkflowResponseStatus();
        }
        public OPSWorkflowResponseStatus ResponseStatus { get; set; }
    }

    public class OpsKeyValue
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }

    public class SendRoomingListToHotelVm
    {
        public string StartDate { get; set; }
        public string Location { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }
        public bool IsSelected { get; set; }
        public string PositionId { get; set; }         
        public bool? PlaceHolder { get; set; } 
        public string FullFormStatus { get; set; } 
    }

    public class OpsPositionRoomPrice
    {
        public string Booking_Id { get; set; }
        public string Position_Id { get; set; }

        public string BookingRooms_Id { get; set; }
        public string PositionPricing_Id { get; set; }

        [Required(ErrorMessage = "*")]
        public int? Req_Count { get; set; }
        public int? Age { get; set; }
        public string ChargeBasis { get; set; }
        public string Status { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal? BudgetPrice { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal? RequestedPrice { get; set; }

        //[Required(ErrorMessage = "*")]
        public decimal? BuyPrice { get; set; }

        public bool ApplyMarkup { get; set; }
        public bool ExcludeFromInvoice { get; set; }
        public bool ConfirmedReqPrice { get; set; }

        public string BuyCurrency_Name { get; set; }

        [Required(ErrorMessage = "*")]
        public string ProductRangeID { get; set; }
        public string ProductRange { get; set; }

        [Required(ErrorMessage = "*")]
        public string PersonTypeID { get; set; }
        public string PersonType { get; set; }

        public int? OnReqQty { get; set; }
        public int? OnAllocQty { get; set; }
        public int? OnFreeSellQty { get; set; }

        //[Required(ErrorMessage = "*")]
        public string OneOffDate { get; set; }

        public string CategoryName { get; set; }
        
        public bool IsNewRow { get; set; }
        public bool? IsAdditionalYN { get; set; }
       
    }

    public class OpsBudgetSupplements
    {
        public string BudgetSupplement_Id { get; set; }
        public string BookingRooms_Id { get; set; }
        public string PositionPricing_Id { get; set; }
        
        public string RoomShortCode { get; set; }
        public string PersonType { get; set; }

        [Required(ErrorMessage = "*")]
        public string ProductRangeID { get; set; }
        public string ProductRange { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal? BudgetSupplementAmount { get; set; }

        [Required(ErrorMessage = "*")]
        public string BudgetSupplementReason { get; set; }
        public bool ApplyMarkUp { get; set; }
        public bool AgentConfirmed { get; set; }
        
        public string BudgetSuppCurrencyId { get; set; }
        public string BudgetSuppCurrencyName { get; set; }
        public string status { get; set; }
        public bool IsNewRow { get; set; }
    }


    public class OpsPositionFOC
    {
        public OpsPositionFOC()
        {
        }

        public string PositionFOC_Id { get; set; }
        public string BuyBookingRoomsId { get; set; }
        public string BuyRoomShortCode { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Value should be greater that 0")]
        public int? BuyQuantity { get; set; }

        public string GetBookingRoomsId { get; set; }
        public string GetRoomShortCode { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Value should be greater that 0")]
        public int? GetQuantity { get; set; }

        public DateTime? CREA_DT { get; set; }
        public DateTime? MODI_DT { get; set; }
        public string CREA_US { get; set; }
        public string MODI_US { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class GetDocTypeByWorkflowReq
    {
        public string ModuleParent { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string PositionStatus { get; set; }
        public List<OpsKeyValue> OpsKeyValue = new List<OpsKeyValue>();
    }
    public class OpsBookingRoomsDetails
    {
        public string RoomType { get; set; }
        public string For { get; set; }
        public int? Age { get; set; }
        public int? Current { get; set; }
        public int? NewLevel { get; set; }
        public string BookingRooms_Id { get; set; }
        public string Status { get; set; }

    }
    public class OpsBookingPaxDetails
    {
        public string PassengerType { get; set; }
        public string PassengerAge { get; set; }
        public int? PassengerQty { get; set; }
        public string BookingPax_ID { get; set; }

    }
    public class FilterRemovedData
    {
        public string RoomName { get; set; }
        public string Age { get; set; }
    }
    public class OpsBookingPositionDetails
    {
        public string ProductType { get; set; }
        public string StartDate { get; set; }
        public string ProductName { get; set; }
        public string SupplierName { get; set; }
        public string Position_ID { get; set; }
        public bool IsSelected { get; set; }


    }
}