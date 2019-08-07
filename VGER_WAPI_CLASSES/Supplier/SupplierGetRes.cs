using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SupplierGetRes
    {
        public List<SupplierList> SupplierList { get; set; }
		public int SupplierTotalCount { get; set; }
		public mCompanies SupplierDetails { get; set; }
        public List<Bookings> NoOfBookings { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public List<BookingCount> BookingCount { get; set; }

        public SupplierGetRes()
        {
            SupplierList = new List<SupplierList>();
            SupplierDetails = new mCompanies();
            NoOfBookings = new List<Bookings>();
            ResponseStatus = new ResponseStatus();
            BookingCount = new List<BookingCount>();
        }
    }

    public class BookingCount
    {
        public string ProductSupplierId { get; set; }
        public string SupplierId { get; set; }
        public string ProductId { get; set; }
        public int TotalCount { get; set; }
    }
}
