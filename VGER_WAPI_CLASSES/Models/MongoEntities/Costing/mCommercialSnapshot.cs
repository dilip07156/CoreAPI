using System;

namespace VGER_WAPI_CLASSES.Models
{
    public class mCommercialSnapshot
    {
        public string QRFPriceID { get; set; } // Identifier for QRFPriceID
        public Commercial Commercial { get; set; }

        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; } = null;
    }
}
