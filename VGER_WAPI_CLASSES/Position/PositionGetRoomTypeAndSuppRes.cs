using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionGetRoomTypeAndSuppRes
    {
        public PositionGetRoomTypeAndSuppRes()
        {
            RoomTypeList = new List<ProductRangeDetails>();
            DefaultRoomslist = new List<ProdCategoryRangeDetails>();
            ResponseStatus = new ResponseStatus();
        }

        public List<ProductRangeDetails> RoomTypeList { get; set; }
        public List<ProdCategoryRangeDetails> DefaultRoomslist { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string ProductId { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
    }
}
