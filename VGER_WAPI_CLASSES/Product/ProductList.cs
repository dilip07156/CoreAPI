using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductListReq
    {
        public string ProdType { get; set; }
        public string Country_Id { get; set; }
        public string City_Id { get; set; }
        public string ProdCode { get; set; }
        public string ProdName { get; set; }
        public bool? IsPlaceHolder { get; set; }
    }   

    public class ProductListRes
    {
        public ProductListRes()
        {
            Products = new List<ProductList>();
            ResponseStatus = new ResponseStatus();
        }
        public List<ProductList> Products { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class ProductList
    {
        public string VoyagerProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool? IsPlaceHolder { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string StarRating { get; set; }
        public string Chain { get; set; }
        public string SupplierId { get; set; }
        public string Supplier { get; set; }
        public ProductLocation LocationInfo { get; set; }
    }

    public class ProductLocation
    {
        public string Lat { get; set; }
        public string Long { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string CityName { get; set; }
        public string CityCode { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
    }

}
