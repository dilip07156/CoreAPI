using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductSRPSearchReq
    {
        public string ProdId { get; set; }
        public string ProdName { get; set; }
        public string ProdCode { get; set; }
        public string StarRating { get; set; }
        public string Chain { get; set; }
        //public string CountryName { get; set; }
        public string CityName { get; set; }
        public string Location { get; set; }
        public string BudgetCategory { get; set; }
        public string ProdType { get; set; }
        public string Status { get; set; }
        public List<string> Facilities { get; set; }
    }

    public class ProductSRPHotelGetRes
    {
        public ProductSRPHotelGetRes()
        {
            ResponseStatus = new ResponseStatus();
            ProductSRPRouteInfo = new List<ProductSRPRouteInfo>();
        }

        public string QRFID { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public List<ProductSRPRouteInfo> ProductSRPRouteInfo { get; set; }
    }

    public class ProductSRPRouteInfo
    {
        public int DayNo { get; set; }
        public string Day { get; set; }
        public int Duration { get; set; }
        public string RoutingDaysID { get; set; }
        public string FromCityId { get; set; }
        public string FromCity { get; set; }
        public string ToCityId { get; set; }
        public string ToCity { get; set; }
        public string ProdId { get; set; }
        public string PositionId { get; set; }
        public ProductSRPDetails ProductSRPDetails { get; set; }
    }

    public class ProductSRPDetails
    {
        #region Product SRP Fields
        public string VoyagerProduct_Id { get; set; }
        public string ProductType_Id { get; set; }
        public bool? Placeholder { get; set; }
        public string ProdName { get; set; }
        public string ProductCode { get; set; }
        public string DefaultSupplierId { get; set; }
        public string DefaultSupplier { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string ProductType { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string PostCode { get; set; }
        public string Status { get; set; }
        public string ProdDesc { get; set; }
        public string Location { get; set; }
        public string StarRating { get; set; }
        public string BdgPriceCategory { get; set; }
        public string Chain { get; set; }
        public string HotelType { get; set; }
        public string HotelImageURL { get; set; }
        public List<Def_Facilities> ProductFacilities { get; set; }
        public List<ProductRoomTypes> Rooms { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
        #endregion

        #region Position Specific Fields
        public string PositionStatus { get; set; }
        public string PositionStatusSCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string AmountCurrency { get; set; }
        public string SupplierName { get; set; }
        public string SupplierContactName { get; set; }
        public string SupplierContactTel { get; set; }
        public string SupplierContactEmail { get; set; }
        #endregion
    }
}
