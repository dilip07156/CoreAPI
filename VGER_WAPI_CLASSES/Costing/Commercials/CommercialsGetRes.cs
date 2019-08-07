using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class CommercialsGetRes
    {
        public string QRFID { get; set; }
        public string QRFPriceId { get; set; }
        public double PercentSoldOptional { get; set; }
        public List<CommercialsData> BareBoneList { get; set; }
        public List<mQRFPositionTotalCost> PositionIncluded { get; set; }
        public List<mQRFPositionTotalCost> PositionOptional { get; set; }
        public List<mQRFPositionTotalCost> PositionSupplement { get; set; }
        public List<QRFExchangeRates> QRFExhangeRates { get; set; }

        public ResponseStatus ResponseStatus { get; set; }

        public CommercialsGetRes()
        {
            BareBoneList = new List<CommercialsData>();
            PositionIncluded = new List<mQRFPositionTotalCost>();
            PositionOptional = new List<mQRFPositionTotalCost>();
            PositionSupplement = new List<mQRFPositionTotalCost>();
            QRFExhangeRates = new List<QRFExchangeRates>();
            ResponseStatus = new ResponseStatus();
        }
    }

    public class CommercialsData
    {
        public string ProductType { get; set; }
        public double CostPrice { get; set; }
        public double SellPrice { get; set; }
        public double ProfitLoss { get; set; }
        public double ProfitPercent { get; set; }
        public string BuyCurrency { get; set; }
        public string SellCurrency { get; set; }
    }
}
