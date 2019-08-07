using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI_CLASSES
{
    public class CostsheetVersion
    {
        public string QRFID { get; set; }
        public string QRFPriceId { get; set; }
        public int VersionId { get; set; }
        public string VersionName { get; set; }
        public string VersionDescription { get; set; }
        public bool IsCurrentVersion { get; set; }

        public DateTime VersionCreateDate { get; set; }

        /// <summary>
        /// QRFPackagePriceList contains all the list of QRF Package Price
        /// </summary>
        public List<mQRFPackagePrice> QRFPackagePriceList { get; set; } = new List<mQRFPackagePrice>();

        /// <summary>
        /// QRFPkgDepartureList conatins Departure,RoomName,QRF Currency details for QRF Price ID wise
        /// </summary>
        public List<DateTime?> QRFPkgDepartureList { get; set; } = new List<DateTime?>();
    }
    /// <summary>
    /// QRFPkgDeparture class defined for taking only Departure,Room,Currency details for QRFPriceID 
    /// </summary>
    public class QRFPkgDeparture
    {
        /// <summary>
        /// Room Name
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// Currency of QRF
        /// </summary>
        public string QRFCurrency { get; set; }

        /// <summary>
        /// Deaprture ID
        /// </summary>
        public long DepartureId { get; set; }

        /// <summary>
        /// Date of Departure
        /// </summary>
        public DateTime? DepartureDate { get; set; }

        /// <summary>
        /// QRF Price ID
        /// </summary>
        public string QRFPriceId { get; set; }
    }
}
