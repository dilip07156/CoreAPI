using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class BookingList
    {
        public string BookingReference { get; set; }
        public string BookingId { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string StatusDesc { get; set; }
        public string BookedDate { get; set; }
        public string GoAheadDate { get; set; }
        public string AgentReference { get; set; }
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public string AgentCode { get; set; }
        public string Agentontact { get; set; }
        public string FileHandler { get; set; }
        public string FileHandlerContact { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Duration { get; set; }
        public string UpdateDate { get; set; }
        public string EndCustometName { get; set; }
        public string BookingName { get; set; }    
        public string PositionId { get; set; }
        public string RoomNo { get; set; }
        public string ProductTemplate { get; set; }
        public string PersonType { get; set; }
        public string ProductRangeId { get; set; }
        public string BookingRoomId { get; set; }
    }

    public class BookingSearchReq
    {
        public string Status { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string GoAheadDate { get; set; }
        public string AgentReference { get; set; }
        public string AgentId { get; set; }
        public string AgentCode { get; set; }
        public string AgentName { get; set; }
        public string UpdateDate { get; set; }
        public bool OnAndAfter { get; set; } = false;
        public string BookingNumber { get; set; }
        public string BookingName { get; set; }
        public string DateType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string PositionId { get; set; }
        public List<string> BookingRoomId { get; set; }
		public int Start { get; set; }
		public int Length { get; set; }
		public string UserId { get; set; }
	}

    public class BookingSearchRes
    {
        public BookingSearchRes()
        {
            Bookings = new List<BookingList>();
            BookingRooms = new List<BookingList>();
            BookingPositionPricing = new List<BookingList>();
            ResponseStatus = new ResponseStatus();
            BookingStatusList = new List<Attributes>();
        }
        public List<BookingList> Bookings { get; set; }
        public List<BookingList> BookingRooms { get; set; }
        public List<BookingList> BookingPositionPricing { get; set; }
        public List<Attributes> BookingStatusList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
		public int BookingTotalCount { get; set; }

	}
}
