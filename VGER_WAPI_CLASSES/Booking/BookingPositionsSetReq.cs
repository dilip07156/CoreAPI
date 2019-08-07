using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
	public class BookingPositionsSetReq
	{
		public BookingPositionsSetReq()
		{
			Position = new Positions();
		}
		public Positions Position { get; set; }
		public string UserEmailId { get; set; }
		public string BookingNumber { get; set; }
		public string PositionId { get; set; }
	}

    public class BookingCancelPositionSetReq
    {         
        public string UserEmailId { get; set; }
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string CancelResoan { get; set; }
        public string PlacerUserId { get; set; }
        public string PageType { get; set; }        
    }
}
