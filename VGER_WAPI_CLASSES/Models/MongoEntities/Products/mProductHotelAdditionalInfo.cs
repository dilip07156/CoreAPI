using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mProductHotelAdditionalInfo
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string ProductId { get; set; }
        public string LocationId { get; set; }
        public string Location { get; set; }
        public string StarRatingId { get; set; }
        public string StarRating { get; set; }
        public string BudgetCategoryId { get; set; }
        public string BudgetCategory { get; set; }
        public string HotelChainId { get; set; }
        public string HotelChain { get; set; }
        public string HotelTypeId { get; set; }
        public string HotelType { get; set; }
        public string Hotel_CreditCards { get; set; }
        public string Hotel_AreaTransportation { get; set; }
        public string Hotel_Restaurants { get; set; }
        public string Hotel_MeetingFacilities { get; set; }
        public string Hotel_Phone { get; set; }
        public string Hotel_Fax { get; set; }
        public string Hotel_Email { get; set; }
        public string Hotel_Website { get; set; }
        public string NoofRooms { get; set; }
        public string Corner { get; set; }
    }
}
