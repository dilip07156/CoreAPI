using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CostsheetGetRes
    {
        public CostsheetGetRes()
        {
            CostsheetVersion = new CostsheetVersion();
            QRFSalesFOC = new List<QRFFOCDetails>();
            QrfPackagePrice = new List<mQRFPackagePrice>();
            QrfNonPackagePrice = new List<mQRFNonPackagedPrice>();
            ResponseStatus = new ResponseStatus();
            QRFPositionTotalCost = new List<mQRFPositionTotalCost>();
        }

        public string QRFID { get; set; }
        public CostsheetVersion CostsheetVersion { get; set; }
        public List<QRFFOCDetails> QRFSalesFOC { get; set; }
        public List<mQRFPackagePrice> QrfPackagePrice { get; set; }
        public List<mQRFNonPackagedPrice> QrfNonPackagePrice { get; set; }
        public List<mQRFPositionTotalCost> QRFPositionTotalCost { get; set; }
        public ResponseStatus ResponseStatus { get; set; }        
    }
}
