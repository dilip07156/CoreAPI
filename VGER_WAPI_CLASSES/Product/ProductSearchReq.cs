using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
  public class ProductSearchReq
    {
        public string Location { get; set; }
        public List<string> ProdType { get; set; }
        public string ProdName { get; set; }
        public string ProdCode { get; set; }
        public string Status { get; set; } 
        public bool? IsPlaceHolder { get; set; }
        public string Chain { get; set; }       
        public string ProductCategoryName { get; set; }
        public string ProductCategoryID { get; set; }
        public string ProductAttributeName { get; set; }
        public string ProductAttributeValue { get; set; }
        public string StarRating { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public List<string> CityList { get; set; }
        public List<string> CountryList { get; set; }
        public List<string> StarRatingList { get; set; }
    } 
}
