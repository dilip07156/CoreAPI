using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class mProposal
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string QRFID { get; set; }
        public string ProposalId { get; set; }
        public string ItineraryId { get; set; } 
        public int Version { get; set; }

        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; }
        public DateTime? EditDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        [BsonIgnoreIfNull(true)]
        public string PriceBreakup { get; set; }

        [BsonIgnoreIfNull(true)]
        public string Inclusions { get; set; }

        [BsonIgnoreIfNull(true)]
        public string Exclusions { get; set; }

        [BsonIgnoreIfNull(true)]
        public string Terms { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CoveringNote { get; set; }

        public string Review { get; set; }

        public ProposalIncludeRegions ProposalIncludeRegions { get; set; }
        //Routing Remaining

        //[BsonIgnoreIfNull(true)]
        //public List<Accomodation> Accomodation { get; set; }

        //[BsonIgnoreIfNull(true)]
        //public string Services { get; set; }
    }

    //public class PriceBreakUp
    //{
    //    public string Type { get; set; }
    //    public string Price { get; set; }
    //    public string Currency { get; set; }

    //    public string CreateUser { get; set; } = "";
    //    public DateTime CreateDate { get; set; } = DateTime.Now;
    //    public string EditUser { get; set; } = "";
    //    public DateTime? EditDate { get; set; } = null;
    //    public bool IsDeleted { get; set; } = false;
    //}

    //public class Terms
    //{
    //    public string Type { get; set; }
    //    public string Value { get; set; }

    //    public string CreateUser { get; set; } = "";
    //    public DateTime CreateDate { get; set; } = DateTime.Now;
    //    public string EditUser { get; set; } = "";
    //    public DateTime? EditDate { get; set; } = null;
    //    public bool IsDeleted { get; set; } = false;
    //}

    //public class InclusionsExclusions
    //{
    //    public string Type { get; set; }
    //    public string Value { get; set; }

    //    public string CreateUser { get; set; } = "";
    //    public DateTime CreateDate { get; set; } = DateTime.Now;
    //    public string EditUser { get; set; } = "";
    //    public DateTime? EditDate { get; set; } = null;
    //    public bool IsDeleted { get; set; } = false;
    //}

    //public class Accomodation
    //{
    //    public string City { get; set; }
    //    public string HotelName { get; set; }
    //    public string NoOfNights { get; set; }
    //    public string Category { get; set; }
    //    public int StarRating { get; set; }
    //    public string Address { get; set; }
    //    public string Telephone { get; set; }
    //    public string Email { get; set; }
    //    public string Website { get; set; }
    //    public string HeroImageURL { get; set; }
    //    public string MapURL { get; set; }
    //    public List<Facilities> Facilities { get; set; } 
    //    public string CreateUser { get; set; } = "";
    //    public DateTime CreateDate { get; set; } = DateTime.Now;
    //    public string EditUser { get; set; } = "";
    //    public DateTime? EditDate { get; set; } = null;
    //    public bool IsDeleted { get; set; } = false;

    //}

    //public class Facilities
    //{
    //    //TODO: Need to take from MongoPush
    //}
}
