using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class TransferDetails
    {
        public TransferProperties TransferProperties { get; set; }

        public string PCTID { get; set; }

        public string STID { get; set; }

        public string FPID { get; set; }

        public string FTID { get; set; }

        public bool IsPCT { get; set; }

        public bool IsST { get; set; }

        public bool IsFP { get; set; }

        public bool IsFT { get; set; }
    }

    public class TransferProperties
    {
        public string PositionId { get; set; }

        public string RoutingDaysID { get; set; }

        public string QRFID { get; set; }

        public int DayID { get; set; }

        public int PositionSequence { get; set; }

        public string DayName { get; set; }

        public string RoutingCity { get; set; }

        public string CreateUser { get; set; }

        public string EditUser { get; set; }

        public bool IsDeleted { get; set; }
    }
}
