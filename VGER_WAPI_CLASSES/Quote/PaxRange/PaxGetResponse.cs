using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PaxGetResponse
    {
        public PaxSlabDetails PaxSlabDetails { get; set; } = new PaxSlabDetails();
        public string Status { get; set; }
        public string QRFID { get; set; }
    }

    public class PaxSlabDetails
    {
        public List<PaxSlabs> PaxSlabs { get; set; } = new List<PaxSlabs>();
        public List<HotelCategories> HotelCategories { get; set; } = new List<HotelCategories>();
        public List<HotelChain> HotelChain { get; set; } = new List<HotelChain>();
        public string HotelFlag { get; set; }
        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
    }

    public class PaxSlabs
    {
        public long PaxSlab_Id { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int DivideByCost { get; set; }
        public string Category { get; set; }
        public string Category_Id { get; set; }
        public string CoachType { get; set; }
        public string CoachType_Id { get; set; }
        public string Brand { get; set; }
        public string Brand_Id { get; set; }
        public int HowMany { get; set; }
        public double? BudgetAmount { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string DeleteUser { get; set; } = "";
        public DateTime? DeleteDate { get; set; } = null;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? EditDate { get; set; } = null;
    }

    public class HotelCategories
    {
        public string VoyagerDefProductCategoryId { get; set; }
        public string Name { get; set; }
    }

    public class HotelChain
    {
        public string AttributeId { get; set; }
        public string Name { get; set; }
    }

    public class PaxSlabList
    {
        public long PaxSlabId { get; set; }
        public string PaxSlab { get; set; }
        public string TourEntityId { get; set; }
    }
}
