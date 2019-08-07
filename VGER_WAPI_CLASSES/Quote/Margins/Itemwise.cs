using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Itemwise
    {
        public Itemwise()
        {
            ItemProperties = new List<ItemProperties>();
            MarginComputed = new MarginComputed();
        }
        public List<ItemProperties> ItemProperties { get; set; }
        public MarginComputed MarginComputed { get; set; }
    }

    public class ItemProperties
    {
        public string ItemID { get; set; }
        public string PositionID { get; set; }
        public string ProductName { get; set; }
        public string VoyagerProductType_Id { get; set; }
        public string Prodtype { get; set; }
        public string MarginUnit { get; set; }
        public double SellingPrice { get; set; }
        public string HowMany { get; set; } 
    }
}
