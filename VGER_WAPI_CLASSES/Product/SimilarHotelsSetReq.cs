using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SimilarHotelsSetReq
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string ProductId { get; set; }
        public string EditUser { get; set; }
        public bool IsClone { get; set; }
        public string Caller { get; set; }
        public List<ProductList> SelectedHotelList { get; set; } = new List<ProductList>();
        public List<ProductList> BlacklistedHotelList { get; set; } = new List<ProductList>();
    }
}
