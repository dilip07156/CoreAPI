using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Models;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Helpers
{
    public class MongoContext
    {

        private readonly IMongoDatabase _database = null;

        public MongoContext(IOptions<MongoSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
            {
                _database = client.GetDatabase(settings.Value.Database);
            }
        }

        public IMongoDatabase database { get { return _database; } }


        public IMongoCollection<mProductHotelAdditionalInfo> mProductHotelAdditionalInfo
        {
            get
            {
                return _database.GetCollection<mProductHotelAdditionalInfo>("mProductHotelAdditionalInfo");
            }
        }

        public IMongoCollection<mDefProductMenu> mDefProductMenu
        {
            get
            {
                return _database.GetCollection<mDefProductMenu>("mDefProductMenu");
            }
        }

        public IMongoCollection<mUsers> mUsers
        {
            get
            {
                return _database.GetCollection<mUsers>("mUsers");
            }
        }

        public IMongoCollection<mStatus> mStatus
        {
            get
            {
                return _database.GetCollection<mStatus>("mStatus");
            }
        }

        public IMongoCollection<mSystem> mSystem
        {
            get
            {
                return _database.GetCollection<mSystem>("mSystem");
            }
        }

        public IMongoCollection<mBookings> mBookings
        {
            get
            {
                return _database.GetCollection<mBookings>("mBookings");
            }
        }

        public IMongoCollection<mGoAhd_Materialisation> mGoAhd_Materialisation
        {
            get
            {
                return _database.GetCollection<mGoAhd_Materialisation>("mGoAhd_Materialisation");
            }
        }

        public IMongoCollection<mBookingItineraryDetail> mBookingItineraryDetail
        {
            get
            {
                return _database.GetCollection<mBookingItineraryDetail>("mBookingItineraryDetail");
            }
        }

        public IMongoCollection<mBookingPax> mBookingPax
        {
            get
            {
                return _database.GetCollection<mBookingPax>("mBookingPax");
            }
        }

        public IMongoCollection<mBookingRooms> mBookingRooms
        {
            get
            {
                return _database.GetCollection<mBookingRooms>("mBookingRooms");
            }
        }

        public IMongoCollection<mBookingPositions> mBookingPositions
        {
            get
            {
                return _database.GetCollection<mBookingPositions>("mBookingPositions");
            }
        }

        public IMongoCollection<mBookingPositionPricing> mBookingPositionPricing
        {
            get
            {
                return _database.GetCollection<mBookingPositionPricing>("mBookingPositionPricing");
            }
        }

        public IMongoCollection<mFOCDilution> mFOCDilution
        {
            get
            {
                return _database.GetCollection<mFOCDilution>("mFOCDilution");
            }
        }

        public IMongoCollection<mCompany> mCompany
        {
            get
            {
                return _database.GetCollection<mCompany>("mCompany");
            }
        }

        public IMongoCollection<mContacts> mContacts
        {
            get
            {
                return _database.GetCollection<mContacts>("mContacts");
            }
        }

        public IMongoCollection<mResort> mResort
        {
            get
            {
                return _database.GetCollection<mResort>("mResort");
            }
        }

        public IMongoCollection<mHotDate> mHotDate
        {
            get
            {
                return _database.GetCollection<mHotDate>("mHotDate");
            }
        }

        public IMongoCollection<mTypeMaster> mTypeMaster
        {
            get
            {
                return _database.GetCollection<mTypeMaster>("mTypeMaster");
            }
        }

        public IMongoCollection<mQuote> mQuote
        {
            get
            {
                return _database.GetCollection<mQuote>("mQuote");
            }
        }  

        public IMongoCollection<mCurrency> mCurrency
        {
            get
            {
                return _database.GetCollection<mCurrency>("mCurrency");
            }
        }

        public IMongoCollection<mSysCounters> mSysCounters
        {
            get
            {
                return _database.GetCollection<mSysCounters>("mSysCounters");
            }
        }

        public IMongoCollection<mQrfFollowUp> mQrfFollowUp
        {
            get
            {
                return _database.GetCollection<mQrfFollowUp>("mQrfFollowUp");
            }
        }

        public IMongoCollection<mProducts> mProducts
        {
            get
            {
                return _database.GetCollection<mProducts>("mProducts");
            }
        }

        public IMongoCollection<mProductType> mProductType
        {
            get
            {
                return _database.GetCollection<mProductType>("mProductType");
            }
        }

        public IMongoCollection<mTransportMaster> mTransportMaster
        {
            get
            {
                return _database.GetCollection<mTransportMaster>("mTransportMaster");
            }
        }

        public IMongoCollection<mTransportCategory> mTransportCategory
        {
            get
            {
                return _database.GetCollection<mTransportCategory>("mTransportCategory");
            }
        }

        public IMongoCollection<mProductCategory> mProductCategory
        {
            get
            {
                return _database.GetCollection<mProductCategory>("mProductCategory");
            }
        }

        public IMongoCollection<mProductRange> mProductRange
        {
            get
            {
                return _database.GetCollection<mProductRange>("mProductRange");
            }
        }

        public IMongoCollection<mProductContract> mProductContract
        {
            get
            {
                return _database.GetCollection<mProductContract>("mProductContract");
            }
        }

        public IMongoCollection<mPricePeriod> mPricePeriod
        {
            get
            {
                return _database.GetCollection<mPricePeriod>("mPricePeriod");
            }
        }

        public IMongoCollection<mProductPrice> mProductPrice
        {
            get
            {
                return _database.GetCollection<mProductPrice>("mProductPrice");
            }
        }

        public IMongoCollection<mProdCatDef> mProdCatDef
        {
            get
            {
                return _database.GetCollection<mProdCatDef>("mProdCatDef");
            }
        }

        public IMongoCollection<mProductAttribute> mProductAttribute
        {
            get
            {
                return _database.GetCollection<mProductAttribute>("mProductAttribute");
            }
        }

        public IMongoCollection<mAttributeValues> mAttributeValues
        {
            get
            {
                return _database.GetCollection<mAttributeValues>("mAttributeValues");
            }
        }

        public IMongoCollection<mPosition> mPosition
        {
            get
            {
                return _database.GetCollection<mPosition>("mPosition");
            }
        }

        public IMongoCollection<mPositionPrice> mPositionPrice
        {
            get
            {
                return _database.GetCollection<mPositionPrice>("mPositionPrice");
            }
        }

        public IMongoCollection<mPositionFOC> mPositionFOC
        {
            get
            {
                return _database.GetCollection<mPositionFOC>("mPositionFOC");
            }
        }

        public IMongoCollection<mProductSupplier> mProductSupplier
        {
            get
            {
                return _database.GetCollection<mProductSupplier>("mProductSupplier");
            }
        }

        public IMongoCollection<mProductFreePlacePolicy> mProductFreePlacePolicy
        {
            get
            {
                return _database.GetCollection<mProductFreePlacePolicy>("mProductFreePlacePolicy");
            }
        }
        public IMongoCollection<mProposal> mProposal
        {
            get
            {
                return _database.GetCollection<mProposal>("mProposal");
            }
        }
        public IMongoCollection<mItinerary> mItinerary
        {
            get
            {
                return _database.GetCollection<mItinerary>("mItinerary");
            }
        }
        public IMongoCollection<mGuesstimate> mGuesstimate
        {
            get
            {
                return _database.GetCollection<mGuesstimate>("mGuesstimate");
            }
        }

        public IMongoCollection<mRoles> mRoles
        {
            get
            {
                return _database.GetCollection<mRoles>("mRoles");
            }
        }

        public IMongoCollection<mUsersInRoles> mUsersInRoles
        {
            get
            {
                return _database.GetCollection<mUsersInRoles>("mUsersInRoles");
            }
        }

        public IMongoCollection<mQRFPrice> mQRFPrice
        {
            get
            {
                return _database.GetCollection<mQRFPrice>("mQRFPrice");
            }
        }
    }
}
