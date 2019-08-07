using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class BookingPositionsSetRes
    {
		public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
		public string PositionId { get; set; }
	}
}
