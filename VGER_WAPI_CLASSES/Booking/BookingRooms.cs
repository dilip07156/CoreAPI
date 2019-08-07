using System;
using System.Collections.Generic;
using System.Text; 

namespace VGER_WAPI_CLASSES
{
    public class BookingRooms
    {
        
        public string Room_Id { get; set; }
        
        public string Type { get; set; }
        
        public int? Count { get; set; }
        
        public string For { get; set; }

        public string Age { get; set; }
    } 

    public class PositionBookingRooms
    {
        public string BookingRooms_ID { get; set; }
        public string Booking_Id { get; set; }
        public string ROOMNO { get; set; }
        public string ProductTemplate_Id { get; set; }
        public string SUBPROD { get; set; }
        public string NAME { get; set; }
        public string Position_Id { get; set; }
        public string ProductRange_Id { get; set; }
        public string PersonType_Id { get; set; }
        public string ApplyMarkup { get; set; }
        public string Category_Id { get; set; }
        public string CategoryName { get; set; }
    }
}
