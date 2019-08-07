using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UpdateOperationDetails
    {
        public string Position_Id { get; set; }
        public string StartTime { get; set; }
        public string PickupLocation { get; set; }
        public string EndTime { get; set; }
        public string DropoffLocation { get; set; }
        public string DriverGuideName { get; set; }
        public string DriverGuideContact { get; set; }
        public string IsConfirmed { get; set; }
    }

    public class UpdateOperationDetails_RQ
    {
        public List<UpdateOperationDetails> UpdateOperationDetails { get; set; }

        public UpdateOperationDetails_RQ()
        {
            UpdateOperationDetails = new List<UpdateOperationDetails>();
        }
    }

    public class UpdateOperationDetails_RS_ByPosition
    {
        public string Position_Id { get; set; }
        public string CurrentPositionStatus { get; set; }
        public List<BookingFixes> PendingBookingActions { get; set; }

        public UpdateOperationDetails_RS_ByPosition()
        {
            PendingBookingActions = new List<BookingFixes>();
        }
    }

    public class UpdateOperationDetails_RS
    {
        public UpdateOperationDetails_RQ Request { get; set; }
        public string CurrentBookingStatus { get; set; }
        public List<UpdateOperationDetails_RS_ByPosition> UpdateOperationDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public UpdateOperationDetails_RS()
        {
            Request = new UpdateOperationDetails_RQ();
            UpdateOperationDetails = new List<UpdateOperationDetails_RS_ByPosition>();
            ResponseStatus = new ResponseStatus();
        }

    }

}
