using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
    public class PositionsFromBookingGetRes
    {
        public PositionsFromBookingGetRes()
        {
            PositionDetails = new List<Positions>();
            Response = new ResponseStatus();
        }
        public string BookingNumber { get; set; }
        public List<Positions> PositionDetails { get; set; }
        public ResponseStatus Response { get; set; }
    }
}
