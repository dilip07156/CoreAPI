using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
	public class QRFSummaryGetRes
	{
		public QRFSummaryGetRes()
		{
			SummaryDetailsInfo = new List<SummaryDetailsInfo>();
			ResponseStatus = new ResponseStatus();
			RoutingInfoCity = new List<VGER_WAPI_CLASSES.RoutingInfoCity>();
		}

		public string QRFID { get; set; }
		public List<SummaryDetailsInfo> SummaryDetailsInfo { get; set; }
		public List<RoutingInfoCity> RoutingInfoCity { get; set; }
		public ResponseStatus ResponseStatus { get; set; }
	}
}
