using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UpdateUserGetReq
    {
        public string QRFID { get; set; }
        public string SalesOfficer { get; set; }
        public string CostingOfficer { get; set; }
        public string ProductAccountant { get; set; }
        public string FileHandler { get; set; }
        public string EditUser { get; set; }
        public string FollowUpCostingOfficer { get; set; }
        public string FollowUpWithClient { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonID { get; set; }
        public string MobileNo { get; set; }
        public string EmailAddress { get; set; }
        public string ModuleName { get; set; }
        public string BookingNumber { get; set; }
    }
}
