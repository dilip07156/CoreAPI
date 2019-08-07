using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionRoomsGetRes
    {
        public PositionRoomsGetRes()
        {
            ProductRangeDetails = new List<ProductRangeDetails>();
            ResponseStatus = new ResponseStatus();
            RoomDetailsInfo = new List<RoomDetailsInfo>();
        }
         
        public List<ProductRangeDetails> ProductRangeDetails { get; set; }
        public List<RoomDetailsInfo> RoomDetailsInfo { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string ProductId { get; set; }
        public string ProductCatId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public string PositionId { get; set; }
    } 
}
