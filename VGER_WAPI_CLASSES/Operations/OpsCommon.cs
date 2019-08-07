using System;

namespace VGER_WAPI_CLASSES
{ 
    public class OperationHeaderInfo
    {
        #region Header Details 
        public decimal ConfirmationPerc { get; set; }
        public string BookingNumber { get; set; }
        public int NoOfDays { get; set; }
        public int NoOfNights { get; set; }
        public string SalesOfficerEmail { get; set; }
        public string CostingOfficerEmail { get; set; }
        public string FileHandlerEmail { get; set; }
        public string ProductAccountantEmail { get; set; }
        public string Destination { get; set; }
        #endregion

        #region Client Key Info
        public string AgentName { get; set; }
        public string ContactName { get; set; }
        public string ContactTel { get; set; }
        public string ContactEmail { get; set; }
        public string TourName { get; set; }
        public string TourType { get; set; }
        #endregion

        #region Timelines
        public string GoAheadDate { get; set; }
        public string HotelConfirmationDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PaymentDueDate { get; set; }
        #endregion

        #region ProductType Info 
        public string ProductType { get; set; }
        public string UIProductType { get; set; }
        public ResponseStatus ResponseStatus = new ResponseStatus();
        #endregion
    }

    public class OpsBookingGetReq
    {
        public string BookingNumber { get; set; }
        public string ProductType { get; set; }
        public string PositionId { get; set; }
    }
}
