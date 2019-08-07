using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AccomodationGetRoomTypeAndSuppRes
    {
        public AccomodationGetRoomTypeAndSuppRes()
        {
            RoomTypeList = new List<ProdCategoryRangeDetails>();
            SupplementList = new List<ProdCategoryRangeDetails>();
            ResponseStatus = new ResponseStatus();
        }

        public List<ProdCategoryRangeDetails> RoomTypeList { get; set; }
        public List<ProdCategoryRangeDetails> SupplementList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string ProductId { get; set; }
        public string SupplierId { get; set; }
    }
}
