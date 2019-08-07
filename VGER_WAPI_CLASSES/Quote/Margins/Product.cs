using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Product
    {
        public Product()
        {
            ProductProperties = new List<ProductProperties>();
            MarginComputed = new MarginComputed();
        }
        public List<ProductProperties> ProductProperties { get; set; }
        public MarginComputed MarginComputed { get; set; }
    }

    public class ProductProperties
    {
        public string ProductID { get; set; }
        public string VoyagerProductType_Id { get; set; }
        public string Prodtype { get; set; }
        public string MarginUnit { get; set; }
        public double SellingPrice { get; set; }
        public string HowMany { get; set; }
        public string ProductMaster { get; set; }
    }
}
