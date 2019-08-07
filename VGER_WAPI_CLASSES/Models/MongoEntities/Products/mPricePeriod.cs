using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mPricePeriod
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerPricePeriod_Id { get; set; }
        public int Pricperiid { get; set; }
        public string Product_Id { get; set; }
        public int Productid { get; set; }
        public string Busitypes { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Datemin { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Datemax { get; set; }
        public string DayCombo_Id { get; set; }
        public string DayComboPattern { get; set; }
        public int Weekdayid { get; set; }
        public string PricePeriodType_Id { get; set; }
        public string Peritypeid { get; set; }
        public string Bfreenight { get; set; }
        public string Sfreenight { get; set; }
        public string Norepeat { get; set; }
        public string Minnights { get; set; }
        public string Pricegrp { get; set; }
        public string Exception { get; set; }
        public string Minqty { get; set; }
        public string Maxqty { get; set; }
        public string Timerowid { get; set; }
        public string DayPattern_Id { get; set; }
        public int Daycombid { get; set; }
        public string Pricecatid { get; set; }
        public string Cxdeadline { get; set; }
        public string ProductContract_Id { get; set; }
        public string Company_Id { get; set; }
        public string MonthPattern { get; set; }
        public string PricePeriodName { get; set; }
        public string BuyPricePeriod_Id { get; set; }
        public string OfferCode_Id { get; set; }
        public string StandardOffer { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}