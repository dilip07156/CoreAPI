using System;
using System.Collections.Generic;
using System.Text;


namespace VGER_WAPI_CLASSES
{
    public class BookingPositions
    {

        public int No { get; set; } = 0;
        public string Position_Id { get; set; }

        public string Status { get; set; }
        public string SupplierConfirmationNumber { get; set; }

        public string Type { get; set; }
        public int? Priority { get; set; }

        public string StartDate { get; set; }
        
        public string StartTime { get; set; }
        
        public string PickupPoint { get; set; }
        public string EndDate { get; set; }
        
        public string EndTime { get; set; }
        
        public string DropOffPoint { get; set; }
        
        public string Duration { get; set; }

        public string ProductType { get; set; }

        public string ProductName { get; set; }
        
        public string ProductCode { get; set; }
        
        
        public string MealPlan { get; set; }
        
        public string Supplier { get; set; }
        
        public string SupplierEmail { get; set; }
        
        public string SupplierPhone { get; set; }
        
        public string SupplierNote { get; set; }
        public string EndSupplier { get; set; }


        public string Country_Id { get; set; }
        public string City_Id { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        
        public string EmptyLeg { get; set; }
        
        public string DriverName { get; set; }
        
        public string DriverContact { get; set; }

        public string LicencePlate { get; set; }

        public string OptionDate { get; set; }
        public string StandardPax { get; set; }
        public string CancellationPolicy { get; set; }
        public string CancellationDeadline { get; set; }
        public string Advice { get; set; }

        public List<BookingPositionPricing> Purchasing { get; set; }
    }
}
