using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Package Price is to be calculated for ONLY CORE positions.  (KeepAs = Included)
    /// </summary>
    public class mQRFPackagePrice
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string QRFPackagePriceId { get; set; }
        public string QRFID { get; set; }
        public string QRFPrice_Id { get; set; }

        public long Departure_Id { get; set; }  // This is saved as INT in QRF collection
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DepartureDate { get; set; }
        public long PaxSlab_Id { get; set; }  // This is saved as INT in QRF collection
        public string PaxSlab { get; set; }

        public string RoomName { get; set; }  // Rooms / Child 
        public int Age { get; set; }  // Child's Age 
        public string BuyCurrencyId { get; set; }
        public string BuyCurrency { get; set; }
        private double buyPrice; // SUM of Buy Prices of All Ranges within position
        public double BuyPrice { get { return this.buyPrice; } set { this.buyPrice = Math.Round(value, 2); } }

        public string QRFCurrency_Id { get; set; }
        public string QRFCurrency { get; set; }

        private double sellPrice; // SUM of Sell Prices of All Ranges within position
        public double SellPrice { get { return this.sellPrice; } set { this.sellPrice = Math.Round(value, 2); } }

        private double profitAmount;  // = TotalSellPrice - (TotalBuyPrice  / ExchangeRate)
        public double ProfitAmount { get { return this.profitAmount; } set { this.profitAmount = Math.Round(value, 2); } }

        private double profitPercentage;
        public double ProfitPercentage { get { return this.profitPercentage; } set { this.profitPercentage = Math.Round(value, 2); } }


        public string Status { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Create_Date { get; set; }
        public string Create_User { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Edit_Date { get; set; }
        public string Edit_User { get; set; }
    }

}
