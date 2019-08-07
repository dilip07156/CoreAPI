using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class BookingPositionPricing
    {
        public string PBR_Id { get; set; }
        public string PP_Id { get; set; }
        public string Quantity { get; set; }
        public string Category { get; set; }
        public string Service { get; set; }
        public string ChargeBasis { get; set; }
        public string Age { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string OneOff { get; set; }
        public string Currency { get; set; }
        public string BudgetPrice { get; set; }
        public string PurchasePrice { get; set; }
        public string PriceConfirmed { get; set; }

        public List<SupplierFOC> FOC { get; set; }
    }

    public class SupplierFOC
    {
        public string FoCBuy { get; set; }
        public string FoCGet { get; set; }
    }
}
