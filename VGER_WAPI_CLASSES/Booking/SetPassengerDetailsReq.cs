using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
   public  class SetPassengerDetailsReq
    {
        public SetPassengerDetailsReq()
        {
            PassengerInfo = new List<PassengerDetails>();
        }
        public List<PassengerDetails> PassengerInfo { get; set; }
        public string Booking_Number { get; set; }
    }
}
