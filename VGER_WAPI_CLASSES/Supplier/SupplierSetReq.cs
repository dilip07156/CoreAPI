using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SupplierSetReq
    {
        public CompanyProducts Product { get; set; } = new CompanyProducts();
        public string Company_Id { get; set; }
        public string ProductSupplier_Id { get; set; }
        public string ProductSupplierSalesAgent_Id { get; set; }
        public string EditUser { get; set; }
        public bool IsAddSalesAgent { get; set; } = false;
        public bool IsRemoveSalesAgent { get; set; } = false;
        public bool IsProduct { get; set; } = false;
		public List<Mappings> SupplierMappings { get; set; } = new List<Mappings>();
		public string Id { get; set; }
		public string PageName { get; set; }
	}
}
