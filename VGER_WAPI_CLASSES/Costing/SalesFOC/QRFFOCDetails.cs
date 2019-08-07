using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFFOCDetails
    {
        public long FOC_Id { get; set; }
        public long DateRangeId { get; set; }
        public string DateRange { get; set; }
        public long PaxSlabId { get; set; }
        public string PaxSlab { get; set; }
        public int FOCSingle { get; set; }
        public int FOCTwin { get; set; }
        public int FOCDouble { get; set; }
        public int FOCTriple { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
    }
}
