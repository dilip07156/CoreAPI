using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Helpers
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

		#region User Role Conatct
		public IMongoCollection<mUsers> mUsers
		{
			get
			{
				return _database.GetCollection<mUsers>("mUsers");
			}
		}

		public IMongoCollection<mSystem> mSystem
		{
			get
			{
				return _database.GetCollection<mSystem>("mSystem");
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
		public IMongoCollection<mCompanies> mCompanies
		{
			get
			{
				return _database.GetCollection<mCompanies>("mCompanies");
			}
		}
		#endregion

		#region Master 
		public IMongoCollection<mTypeMaster> mTypeMaster
		{
			get
			{
				return _database.GetCollection<mTypeMaster>("mTypeMaster");
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

		/// <summary>
		/// city master
		/// </summary>
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

		/// <summary>
		/// meal plan status
		/// </summary>
		public IMongoCollection<mDefMealPlan> mDefMealPlan
		{
			get
			{
				return _database.GetCollection<mDefMealPlan>("mDefMealPlan");
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

        public IMongoCollection<mMealType> mMealType
        {
            get
            {
                return _database.GetCollection<mMealType>("mMealType");
            }
        }
        #endregion

        #region Settings

        public IMongoCollection<mSalesPipelineRoles> mSalesPipelineRoles
		{
			get
			{
				return _database.GetCollection<mSalesPipelineRoles>("mSalesPipelineRoles");
			}
		}

		#endregion

		#region Product details
		public IMongoCollection<mProducts> mProducts
		{
			get
			{
				return _database.GetCollection<mProducts>("mProducts");
			}
		}

		public IMongoCollection<mProducts_Lite> mProducts_Lite
		{
			get
			{
				return _database.GetCollection<mProducts_Lite>("mProducts_Lite");
			}
		}

		public IMongoCollection<Products> Products
		{
			get
			{
				return _database.GetCollection<Products>("Products");
			}
		}

		public IMongoCollection<mProductType> mProductType
		{
			get
			{
				return _database.GetCollection<mProductType>("mProductType");
			}
		}

		public IMongoCollection<mProductCategory> mProductCategory
		{
			get
			{
				return _database.GetCollection<mProductCategory>("mProductCategory");
			}
		}

		public IMongoCollection<mProductLevelAttribute> mProductLevelAttribute
		{
			get
			{
				return _database.GetCollection<mProductLevelAttribute>("mProductLevelAttribute");
			}
		}

		public IMongoCollection<mProductRange> mProductRange
		{
			get
			{
				return _database.GetCollection<mProductRange>("mProductRange");
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

		public IMongoCollection<mProductContract> mProductContract
		{
			get
			{
				return _database.GetCollection<mProductContract>("mProductContract");
			}
		}

        public IMongoCollection<Contracts> ProductContracts
        {
            get
            {
                return _database.GetCollection<Contracts>("ProductContracts");
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

		public IMongoCollection<mProductHotelAdditionalInfo> mProductHotelAdditionalInfo
		{
			get
			{
				return _database.GetCollection<mProductHotelAdditionalInfo>("mProductHotelAdditionalInfo");
			}
		}

		public IMongoCollection<mServiceDuration> mServiceDuration
		{
			get
			{
				return _database.GetCollection<mServiceDuration>("mServiceDuration");
			}
		}

		public IMongoCollection<mPricePeriod> mPricePeriod
		{
			get
			{
				return _database.GetCollection<mPricePeriod>("mPricePeriod");
			}
		}

		public IMongoCollection<mMailServerConfiguration> mMailServerConfiguration
		{
			get
			{
				return _database.GetCollection<mMailServerConfiguration>("mMailServerConfiguration");
			}
		}
		#endregion

		#region Quote details
		public IMongoCollection<mQuote> mQuote
		{
			get
			{
				return _database.GetCollection<mQuote>("mQuote");
			}
		}

		public IMongoCollection<mQrfFollowUp> mQrfFollowUp
		{
			get
			{
				return _database.GetCollection<mQrfFollowUp>("mQrfFollowUp");
			}
		}
		#endregion

		#region Position details
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
		#endregion

		#region costing details
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

		public IMongoCollection<mQRFPrice> mQRFPrice
		{
			get
			{
				return _database.GetCollection<mQRFPrice>("mQRFPrice");
			}
		}

		public IMongoCollection<mQRFPosition> mQRFPosition
		{
			get
			{
				return _database.GetCollection<mQRFPosition>("mQRFPosition");
			}
		}

		public IMongoCollection<mExchangeRate> mExchangeRate
		{
			get
			{
				return _database.GetCollection<mExchangeRate>("mExchangeRate");
			}
		}

		public IMongoCollection<mExchangeRateDetail> mExchangeRateDetail
		{
			get
			{
				return _database.GetCollection<mExchangeRateDetail>("mExchangeRateDetail");
			}
		}

		public IMongoCollection<mQRFPositionTotalCost> mQRFPositionTotalCost
		{
			get
			{
				return _database.GetCollection<mQRFPositionTotalCost>("mQRFPositionTotalCost");
			}
		}

		public IMongoCollection<mQRFPositionPrice> mQRFPositionPrice
		{
			get
			{
				return _database.GetCollection<mQRFPositionPrice>("mQRFPositionPrice");
			}
		}

		public IMongoCollection<mQRFPackagePrice> mQRFPackagePrice
		{
			get
			{
				return _database.GetCollection<mQRFPackagePrice>("mQRFPackagePrice");
			}
		}

		public IMongoCollection<mQRFNonPackagedPrice> mQRFNonPackagedPrice
		{
			get
			{
				return _database.GetCollection<mQRFNonPackagedPrice>("mQRFNonPackagedPrice");
			}
		}

		public IMongoCollection<mTermsAndConditions> mTermsAndConditions
		{
			get
			{
				return _database.GetCollection<mTermsAndConditions>("mTermsAndConditions");
			}
		}

		public IMongoCollection<Bookings> Bookings
		{
			get
			{
				return _database.GetCollection<Bookings>("Bookings");
			}
		}

		public IMongoCollection<mBookings> mBookings
		{
			get
			{
				return _database.GetCollection<mBookings>("mBookings");
			}
		}

		public IMongoCollection<mBookingRooms> mBookingRooms
		{
			get
			{
				return _database.GetCollection<mBookingRooms>("mBookingRooms");
			}
		}

		public IMongoCollection<mStatus> mStatus
		{
			get
			{
				return _database.GetCollection<mStatus>("mStatus");
			}
		}

		public IMongoCollection<mDefStartPage> mDefStartPage
		{
			get
			{
				return _database.GetCollection<mDefStartPage>("mDefStartPage");
			}
		}

        public IMongoCollection<mDefPersonType> mDefPersonType
        {
            get
            {
                return _database.GetCollection<mDefPersonType>("mDefPersonType");
            }
        }

        public IMongoCollection<mBookingPositionPricing> mBookingPositionPricing
		{
			get
			{
				return _database.GetCollection<mBookingPositionPricing>("mBookingPositionPricing");
			}
		}

		public IMongoCollection<mGenericImages> mGenericImages
		{
			get
			{
				return _database.GetCollection<mGenericImages>("mGenericImages");
			}
		}
		public IMongoCollection<mBookingDocuments> mBookingDocuments
		{
			get
			{
				return _database.GetCollection<mBookingDocuments>("mBookingDocuments");
			}
		}
		public IMongoCollection<mPositionPriceQRF> mPositionPriceQRF
		{
			get
			{
				return _database.GetCollection<mPositionPriceQRF>("mPositionPriceQRF");
			}
		}
		public IMongoCollection<mQRFPositionFOC> mQRFPositionFOC
		{
			get
			{
				return _database.GetCollection<mQRFPositionFOC>("mQRFPositionFOC");
			}
		}

		public IMongoCollection<mMarkups> mMarkups
		{
			get
			{
				return _database.GetCollection<mMarkups>("mMarkups");
			}
		}

		public IMongoCollection<mDefDocumentTypes> mDefDocumentTypes
		{
			get
			{
				return _database.GetCollection<mDefDocumentTypes>("mDefDocumentTypes");
			}
		}

		public IMongoCollection<mProductSupplierOperatingMkt> mProductSupplierOperatingMkt
		{
			get
			{
				return _database.GetCollection<mProductSupplierOperatingMkt>("mProductSupplierOperatingMkt");
			}
		}

		public IMongoCollection<mProductSupplierSalesMkt> mProductSupplierSalesMkt
		{
			get
			{
				return _database.GetCollection<mProductSupplierSalesMkt>("mProductSupplierSalesMkt");
			}
		}

		public IMongoCollection<mBusinessRegions> mBusinessRegions
		{
			get
			{
				return _database.GetCollection<mBusinessRegions>("mBusinessRegions");
			}
		}
		#endregion

		#region Agent Approval Pipline
		public IMongoCollection<mGoAhead> mGoAhead
		{
			get
			{
				return _database.GetCollection<mGoAhead>("mGoAhead");
			}
		}

		public IMongoCollection<mDateTest> mDateTest
		{
			get
			{
				return _database.GetCollection<mDateTest>("mDateTest");
			}
		}
		#endregion

		#region DocumentStore
		public IMongoCollection<mDocumentStore> mDocumentStore
		{
			get
			{
				return _database.GetCollection<mDocumentStore>("mDocumentStore");
			}
		}
        #endregion

        #region mApplications
        public IMongoCollection<mApplications> mApplications
        {
            get
            {
                return _database.GetCollection<mApplications>("mApplications");
            }
        }
        #endregion

        #region mIntegrationApplicationData
        public IMongoCollection<mIntegrationApplicationData> mIntegrationApplicationData
        {
            get
            {
                return _database.GetCollection<mIntegrationApplicationData>("mIntegrationApplicationData");
            }
        }
        #endregion

        #region mIntegrationPlatform
        public IMongoCollection<mIntegrationPlatform> mIntegrationPlatform
        {
            get
            {
                return _database.GetCollection<mIntegrationPlatform>("mIntegrationPlatform");
            }
        }
        #endregion

       

        #region mMISMapping
        public IMongoCollection<mMISMapping> mMISMapping
        {
            get
            {
                return _database.GetCollection<mMISMapping>("mMISMapping");
            }
        }
        #endregion

        public IMongoCollection<Workflow_Actions> Workflow_Actions
        {
            get
            {
                return _database.GetCollection<Workflow_Actions>("Workflow_Actions");
            }
        }

        public IMongoCollection<EmergencyContacts> mEmergencyContacts
        {
            get
            {
                return _database.GetCollection<EmergencyContacts>("mEmergencyContacts");
            }
        }
        public IMongoCollection<mProductTemplates> mProductTemplates
        {
            get
            {
                return _database.GetCollection<mProductTemplates>("mProductTemplates");
            }
        }
    }
}
