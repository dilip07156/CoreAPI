using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UpdatePositionProduct
    {
        public string Position_Id { get; set; }
        public string Product_Id { get; set; }
        public bool? IsConfirmed { get; set; }
    }

    public class UpdatePositionProduct_RQ
    {
        public List<UpdatePositionProduct> UpdatePositionProduct { get; set; }

        public UpdatePositionProduct_RQ()
        {
            UpdatePositionProduct = new List<UpdatePositionProduct>();
        }
    }

    public class UpdatePositionProduct_RS_ByProduct
    {
        public string Position_Id { get; set; }
        public string Product_Id { get; set; }
        public string CurrentPositionStatus { get; set; }
        //public List<BookingFixes> PendingBookingActions { get; set; }

        public UpdatePositionProduct_RS_ByProduct()
        {
            //PendingBookingActions = new List<BookingFixes>();
        }
    }

    public class UpdatePositionProduct_RS
    {
        public UpdatePositionProduct_RQ Request { get; set; }
        public List<UpdatePositionProduct_RS_ByProduct> UpdatePositionProduct { get; set; }
        public string CurrentBookingStatus { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public UpdatePositionProduct_RS()
        {
            Request = new UpdatePositionProduct_RQ();
            ResponseStatus = new ResponseStatus();
        }
    }
}
