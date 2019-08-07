using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFMargins
    {
        public string SelectedMargin { get; set; } = "Package";
        public QRFMarginPackage Package { get; set; } = new QRFMarginPackage();
        public QRFMarginProduct Product { get; set; } = new QRFMarginProduct();
        public QRFMarginItem Item { get; set; } = new QRFMarginItem();
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; } = new DateTime();
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
    }

    public class QRFMarginPackage
    {
        public List<QRFMarginPackageProperties> PackageProperties { get; set; }= new List<QRFMarginPackageProperties>();
        public QRFMarginComputed MarginComputed { get; set; } = new QRFMarginComputed();
    }

    public class QRFMarginPackageProperties
    {
        public string PackageID { get; set; }
        public string ComponentName { get; set; }
        public double SellingPrice { get; set; }
        public string MarginUnit { get; set; }
    }

    public class QRFMarginComputed
    {
        public string TotalCost { get; set; }
        public string TotalLeadersCost { get; set; }
        public string Upgrade { get; set; }
        public string MarkupType { get; set; }
    }

    public class QRFMarginProduct
    {
        public List<QRfMarginProductProperties> ProductProperties { get; set; } = new List<QRfMarginProductProperties>();
        public QRFMarginComputed MarginComputed { get; set; } = new QRFMarginComputed();
    }

    public class QRfMarginProductProperties
    {
        public string ProductID { get; set; }
        public string VoyagerProductType_Id { get; set; }
        public string Prodtype { get; set; }
        public string MarginUnit { get; set; }
        public double SellingPrice { get; set; }
        public string HowMany { get; set; }
    }

    public class QRFMarginItem
    {
        public List<QRfMarginItemProperties> ItemProperties { get; set; } = new List<QRfMarginItemProperties>();
        public QRFMarginComputed MarginComputed { get; set; } = new QRFMarginComputed();
    }

    public class QRfMarginItemProperties
    {
        public string ItemID { get; set; }
        public string PositionID { get; set; }
        public string ProductName { get; set; }
        public string VoyagerProductType_Id { get; set; }
        public string Prodtype { get; set; }
        public string MarginUnit { get; set; }
        public double SellingPrice { get; set; }
        public string HowMany { get; set; }
        public string KeepAs { get; set; }
    }
}
