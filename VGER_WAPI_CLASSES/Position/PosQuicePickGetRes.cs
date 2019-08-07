using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PosQuicePickGetRes
    {
        public PosQuicePickGetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public List<PosQuickPickList> PosQuickPickList { get; set; } = new List<PosQuickPickList>();


        //public List<ProductSearchDetails> ProductList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string QRFID { get; set; }
        //public List<AttributeValues> DaysList { get; set; }
        //public List<RoutingDays> RoutingDays { get; set; }
        //public List<RoutingInfo> RoutingInfo { get; set; }
        //public List<ProductType> ProductType { get; set; }
    }

    public class PosQuickPickList
    {
        public string CityID { get; set; }
        public string CityName { get; set; }
        public List<PosQuickPickProductList> PosQuickPickProductList { get; set; } = new List<PosQuickPickProductList>();
    }

    public class PosQuickPickProductList
    {
        public bool IsSelected { get; set; }
        public string ProdId { get; set; }
        public string ProdCode { get; set; }
        public string ProdName { get; set; }
        public string ProdType { get; set; }
        public string ProdTypeId { get; set; }
        public string DayName { get; set; }
        public string ActivityDayNo { get; set; }
        public string SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string StartTime { get; set; }
        public string PositionId { get; set; }
    }
}
