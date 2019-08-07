using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
	public class OpsFOCSetReq
	{
		public OpsFOCSetReq()
		{
			PositionFoc = new List<PositionFOC>();
		}

		public string BookingNo { get; set; }
		public string Position_Id { get; set; }
		public List<PositionFOC> PositionFoc;
	}

	public class OpsFOCSetRes
	{
		public OpsFOCSetRes()
		{
			ResponseStatus = new ResponseStatus();
		}

		public ResponseStatus ResponseStatus { get; set; }
	}
}
