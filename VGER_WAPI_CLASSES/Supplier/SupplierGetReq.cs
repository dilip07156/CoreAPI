using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SupplierGetReq
    {
        public string ProductSupplierId { get; set; }
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public Guid? CountryId { get; set; }
        public string Status { get; set; }
        public Guid? CityId { get; set; }
        public string ProductType { get; set; }
        public string UserId { get; set; }
        public string CompanyId { get; set; }
        public string EditUser { get; set; }
        public List<BookingCount> bookingCount { get; set; } = new List<BookingCount>();
		public int Start { get; set; }
		public int Length { get; set; }
		public string PageName { get; set; }
	}
}
