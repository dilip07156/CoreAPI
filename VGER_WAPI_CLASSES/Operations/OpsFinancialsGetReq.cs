using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
	public class OpsFinancialsGetReq
	{
		public string BookingNumber { get; set; }
		public string PositionId { get; set; }
	}

	public class OpsFinancialsGetRes
	{
		public OpsFinancialsGetRes()
		{
			ResponseStatus = new ResponseStatus();
			FinancialDetail = new List<FinancialDetail>();
		}
		public ResponseStatus ResponseStatus { get; set; }
		public List<FinancialDetail> FinancialDetail { get; set; }
		public string BookingNumber { get; set; }
		public string PositionId { get; set; }
		public string TotalBuyPrice { get; set; }
		public string TotalBuyCurrency { get; set; }
		public string TotalSellPrice { get; set; }
		public string TotalSellCurrency { get; set; }
		public string TotalGPPercent { get; set; }
		public string TotalGPAmount { get; set; }
	}

	public class FinancialDetail
	{		
		public string Date { get; set; }
		public string Item { get; set; }
		public string Quantity { get; set; }
		public string ChargeBy { get; set; }
		public string Buy { get; set; }
		public string Rate { get; set; }
		public string Total { get; set; }
		public string Basis { get; set; }
		public string Value { get; set; }
		public string Sell { get; set; }
		public string SValue { get; set; }
		public string GPPercent { get; set; }
		public string GPAmount { get; set; }
	}
}
