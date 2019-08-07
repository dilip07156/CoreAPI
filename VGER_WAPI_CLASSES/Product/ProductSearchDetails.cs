using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductSearchDetails
    {
        public string VoyagerProduct_Id { get; set; }
        public string ProdCode { get; set; }
        public string ProdName { get; set; }
        public string ProdType { get; set; }
        public string ProdTypeId { get; set; }
        public string Status { get; set; }
        public ProdLocation ProdLocation { get; set; }
        public string StarRatingId { get; set; }
        public string StarRating { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string LocationId { get; set; }
        public string Location { get; set; }
        public string Chain { get; set; }
        public string ChainId { get; set; }
        public string DefaultSupplierId { get; set; }
        public string DefaultSupplier { get; set; }
        public bool? PlaceHolder { get; set; }
    }

    public class ProdLocation
    {
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string CityName { get; set; }
        public string CityCode { get; set; }
    }


}
