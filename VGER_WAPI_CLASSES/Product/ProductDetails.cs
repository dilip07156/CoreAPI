using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductDetailReq
    {
        public string Product_Id { get; set; }
    }

    public class ProductDetailRes
    {
        public ProductDetailRes()
        {
            Product = new ProductDetails();
            ResponseStatus = new ResponseStatus();
        }
        public ProductDetails Product { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
    public class ProductDetails
    {
        public string Product_Id { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public bool? IsPlaceHolder { get; set; }
        public string Metro { get; set; }
        public string Description { get; set; }
        public ProductLocation Location { get; set; }
        public HotelInfo HotelInfo { get; set; }
        public List<ProductCategoryInfo> Cateogry { get; set; }

    }

    public class HotelInfo
    { 
        public string Location { get; set; }
        public string Category { get; set; }
        public string StarRating { get; set; }
        public string Chain { get; set; }
        public string HotelType { get; set; }
        public string Corner { get; set; }
    }

    public class ProductCategoryInfo
    {
        public string Category_Id { get; set; }
        public string Name { get; set; }
        public bool? IsDefault { get; set; } = false;
        public string ParentCategory { get; set; }

        public List<ProductRangesInfo> Ranges { get; set; }
    }

    public class ProductRangesInfo
    {
        public string Range_Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ChargeBasis { get; set; }
        public string MinAge { get; set; }
        public string MaxAge { get; set; }
        public string Menu { get; set; }
        public bool? isSupplement { get; set; } = false;
        public int? Capacity { get; set; }
    }
}
