using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
   
    public class mQRFPositionPrice
    {

        [BsonId]
        public ObjectId _Id { get; set; }
        public string QRFPositionPriceID { get; set; }
        public string QRFID { get; set; }
        public string QRFPrice_Id { get; set; }
        public string QRFCostForPositionID { get; set; }
        public string PositionId { get; set; }        

        public string ProductRange_Id { get; set; }
        public string ProductRange { get; set; } // Product Template
        public string PersoneType { get; set; }
        public int Age { get; set; }
        public bool IsAdditional { get; set; }

        public string BuyCurrency { get; set; }

        private double totalBuyPrice; // TOTAL BUY price = PRICE in Guestimate * DURATION 
        public double TotalBuyPrice { get { return this.totalBuyPrice; } set { this.totalBuyPrice = Math.Round(value, 2); } }
        
        private double totalMarkup; // Calculated from Margin Details
        public double TotalMarkup { get { return this.totalMarkup; } set { this.totalMarkup = Math.Round(value, 2); } }

        private double fOCInBuyCurrency; // FOC calculation for that Range
        public double FOCInBuyCurrency { get { return this.fOCInBuyCurrency; } set { this.fOCInBuyCurrency = Math.Round(value, 2); } }

        private double totalSellPriceInBuyCurrency; // TotalSellPriceInBuyCurrency = TotalBuyPrice + FOCInBuyCurrency + Claculated Markup
        public double TotalSellPriceInBuyCurrency { get { return this.totalSellPriceInBuyCurrency; } set { this.totalSellPriceInBuyCurrency = Math.Round(value, 2); } }

        public string QRFCurrency { get; set; }
        private double totalSellPrice; // TOTAL SELL price = (TotalSellPriceInBuyCurrency / ExchangeRate) 
        public double TotalSellPrice { get { return this.totalSellPrice; } set { this.totalSellPrice = Math.Round(value, 2); } }
        /// <summary>
        /// ExchangeRate = BuyCurrency==ToCurrency, QRFCurrency = FromCurrency in ExchangeRate in QRFPrice
        /// </summary>

        public bool IsDeleted { get; set; } = false;
        public string Status { get; set; }
        public DateTime Create_Date { get; set; }
        public string Create_User { get; set; }
        public DateTime Edit_Date { get; set; } = DateTime.Now;
        public string Edit_User { get; set; } = null;
    }

    /// <summary>
    /// This is to derive QRFNotPackagePrice
    /// </summary>
    public class mQRFPositionTotalCost
    {
        [BsonId] 
        public ObjectId _Id { get; set; }

        public string QRFCostForPositionID { get; set; }
        public string QRFID { get; set; }
        public string QRFPrice_Id { get; set; }

        public string PositionId { get; set; }
        public string PositionType { get; set; }
        public string PositionKeepAs { get; set; }  // CORE / SUPPLEMNT / OPTIONAL
        public string ProductId { get; set; }
        public string ProductName { get; set; }

        public long Departure_Id { get; set; }  // This is saved as INT in QRF collection
        public DateTime? DepartureDate { get; set; }
        public long PaxSlab_Id { get; set; }  // This is saved as INT in QRF collection
        public string PaxSlab { get; set; }
        public string BuyCurrencyId { get; set; }
        public string BuyCurrency { get; set; }

        private double totalBuyPrice; // SUM of Buy Prices of All Ranges within position
        public double TotalBuyPrice { get { return this.totalBuyPrice; } set { this.totalBuyPrice = Math.Round(value, 2); } }

        public string QRFCurrency_Id { get; set; }
        public string QRFCurrency { get; set; }

        private double fOCInBuyCurrency; // FOC calculation for that Range
        public double FOCInBuyCurrency { get { return this.fOCInBuyCurrency; } set { this.fOCInBuyCurrency = Math.Round(value, 2); } }

        private double totalSellPrice; // SUM of Sell Prices of All Ranges within position
        public double TotalSellPrice { get { return this.totalSellPrice; } set { this.totalSellPrice = Math.Round(value, 2); } }
        
        private double profitAmount;  // = TotalSellPrice - (TotalBuyPrice  / ExchangeRate)
        public double ProfitAmount { get { return this.profitAmount; } set { this.profitAmount = Math.Round(value, 2); } }

        private double profitPercentage;  // = (ProfitAmount * 100) / TotalSellPrice
        public double ProfitPercentage { get { return this.profitPercentage; } set { this.profitPercentage = Math.Round(value, 2); } }


        public string Status { get; set; }
        public DateTime Create_Date { get; set; }
        public string Create_User { get; set; }
        public DateTime Edit_Date { get; set; } = DateTime.Now;
        public string Edit_User { get; set; } = null;

    }
}
