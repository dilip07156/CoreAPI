using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UpdatePurchaseDetails
    {
        public string Position_Id { get; set; }
        public string PBR_Id { get; set; }
        public string PP_Id { get; set; }
        public int? Quantity { get; set; }
        public decimal? BuyPrice { get; set; }
        public bool? IsConfirmed { get; set; }
    }

    public class PurchaseOrderBreakup
    {
        public string Position_Id { get; set; }
        public string PBR_Id { get; set; }
        public string PP_Id { get; set; }
        public DateTime? Date { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitBuyPrice { get; set; }
        public decimal? TotalBuyPrice { get; set; }
    }

    public class PurchaseOrderTotal
    {
        public string Position_Id { get; set; }
        public string PBR_Id { get; set; }
        public string PP_Id { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitBuyPrice { get; set; }
        public decimal? TotalBuyPrice { get; set; }
        public List<PurchaseOrderBreakup> Breakup { get; set; }

        public PurchaseOrderTotal()
        {
            Breakup = new List<PurchaseOrderBreakup>();
        }
    }

    public class UpdatePurchaseDetails_RQ
    {
        public List<UpdatePurchaseDetails> UpdatePurchaseDetails { get; set; }

        public UpdatePurchaseDetails_RQ()
        {
            UpdatePurchaseDetails = new List<UpdatePurchaseDetails>();
        }
    }

    public class UpdatePurchaseDetails_RS_ByPosition
    {
        public string Position_Id { get; set; }
        public string PBR_Id { get; set; }
        public string PP_Id { get; set; }
        public string CurrentPositionStatus { get; set; }
        //public List<BookingFixes> PendingBookingActions { get; set; }
        public List<PurchaseOrderTotal> PurchaseOrder { get; set; }

        public UpdatePurchaseDetails_RS_ByPosition()
        {
            //PendingBookingActions = new List<BookingFixes>();
            PurchaseOrder = new List<PurchaseOrderTotal>();
        }

    }

    public class UpdatePurchaseDetails_RS
    {
        public UpdatePurchaseDetails_RQ Request { get; set; }
        public List<UpdatePurchaseDetails_RS_ByPosition> UpdatePurchaseDetails { get; set; }
        public string CurrentBookingStatus { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public UpdatePurchaseDetails_RS()
        {
            Request = new UpdatePurchaseDetails_RQ();
            UpdatePurchaseDetails = new List<UpdatePurchaseDetails_RS_ByPosition>();
            ResponseStatus = new ResponseStatus();
        }

    }
}
