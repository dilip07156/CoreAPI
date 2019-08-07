using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.QRFSummary;

namespace VGER_WAPI.Repositories
{
    public interface IQuoteRepository
    {
		#region Agent
		List<AgentProperties> GetAgentCompanies(AgentCompanyReq request);

        IQueryable<dynamic> GetAgentCompaniesfrommCompanies(AgentCompanyReq request);

        IQueryable<dynamic> GetSuppliersfrommCompanies(AgentCompanyReq request);

        CompanyDetailsRes GetCompanyDetails();

        IQueryable<dynamic> GetContactsForAgentCompany(AgentContactReq request);

        string CheckDuplicateQRFTourName(AgentContactReq request);

        Task<string> InsertUpdateQRFAgentDetails(QUOTEAgentSetReq request);

        string GetValueofAttributeFromMaster(mTypeMaster request, string AttributeName, string value);

        Task<IList<QuoteSearchDetails>> GetQRFAgentDetailsBySearchCriteria(QuoteSearchReq request);

        Task<QuoteAgentGetProperties> GetQRFAgentDetailsByQRFID(QuoteAgentGetReq request);

        Task<AgentContactDetailsProperties> GetContactDetailsByAgentAndContactID(AgentContactDetailsReq request);

		Task<DivisionGetRes> GetDivision(QuoteSearchReq request);
		#endregion

		#region Departures

		Task<DepartureDateSetResponse> SetDepartureDatesForQRF_Id(DepartureDateSetRequest request);

        DepartureDateGetResponse GetDepartureDatesForQRF_Id(DepartureDateGetRequest req);

        #endregion

        #region PaxSlabDetails

        PaxGetResponse GetPaxSlabDetailsForQRF_Id(PaxGetRequest req);

        Task<PaxSetResponse> SetPaxSlabDetailsForQRF_Id(PaxSetRequest request);

        #endregion

        #region Routing Info
        Task<List<RoutingInfo>> GetQRFRouteDetailsByQRFID(RoutingGetReq request);

        Task<string> InsertUpdateQRFRouteDetails(RoutingSetReq request);

        Task<bool> AddHotels(string username, string QRFID, bool IsOverwriteExtPos);
        #endregion

        #region RoutingDays 
        Task<RoutingDaysGetRes> InsertUpdateQRFRoutingDays(RoutingDaysSetReq request);

        Task<RoutingDaysGetRes> GetQRFRoutingDays(RoutingDaysGetReq request);
        #endregion        

        #region Margin 
        Task<Margins> GetQRFMarginDetailsByQRFID(MarginGetReq request);

        Task<string> InsertUpdateQRFMarginDetails(MarginSetReq request);

        List<Currency> GetActiveCurrencyList(CurrencyGetReq request);
        #endregion

        #region FOC

        FOCGetResponse GetFOCDetailsForQRF_Id(PaxGetRequest req);

        Task<PaxSetResponse> SetFOCDetailsForQRF_Id(FOCSetRequest request);

        #endregion

        #region TourEntities
        Task<TourEntitiesSetRes> SetTourEntities(TourEntitiesSetReq request);

        Task<TourEntitiesGetRes> GetTourEntities(TourEntitiesGetReq request);
        #endregion

        #region Meals
        Task<MealSetRes> SetMeals(MealSetReq request);

        Task<MealGetRes> GetMeals(MealGetReq request);
        #endregion

        #region FollowUp
        Task<FollowUpSetRes> SetFollowUpForQRF(FollowUpSetReq req);

        FollowUpGetRes GetFollowUpForQRF(FollowUpGetReq req);

        FollowUpMasterGetRes GetFollowUpMasterData(FollowUpGetReq req);
        #endregion

        Task<bool> ChcekLinkedQRFsExist(string QRFID);

        Task<List<LinkedQRFsData>> GetLinkedQRFs(LinkedQRFsGetReq request);

        Task<CommonResponse> SetQuoteRejectOpportunity(QuoteRejectOpportunityReq req);

        AgentProperties GetAgentCompaniesByID(string id);
        ContactProperties GetContactsForAgentCompanyByID(string id);

        #region 3rd party QuoteRejectOpportunity
        Task<OpportunityPartnerRes> SetPartnerQuoteRejectOpportunity(ManageOpportunityReq req);

        #endregion

        #region 3rd party Quote

        Task<QuoteThirdPartyGetRes> GetPartnerQuoteDetails(QuoteThirdPartyGetReq request);

        #endregion

        #region FollowUp Quote
        //IQueryable<dynamic> GetFollowUpForQRF_Id(QrfFollowUpRequest request);

        //IQueryable<dynamic> GetFollowUpForFollowUp_Id(QrfFollowUpRequest request);

        //Task<bool> SetFollowUpForQRF_Id(QrfFollowUpSetRequest req);

        //FollowUpItem GetFollowUpByQuoteSearchCriteria(QuoteSearchReq request, string strQRFID);
        #endregion
            
        #region Update ValidForAcceptance field in mQuote and mQRFPrice collection
        Task<ResponseStatus> UpdateValidForAcceptance(QuoteGetReq req);
        #endregion
        #region GetQrfDocuments
        Task<QrfDocumentGetResponse> GetQuoteQrfDocumentsDetails(QrfDocumentGetReq req);
        Task<QrfDocumentPostResponse> SaveQRFDocumentsForQrfId(QrfDocumentPostRequest request);
        Task<mQuote> GetQuoteDetails(string QrfId);
        #endregion

        #region MSDynamics for Opportunity Bookins and Quote

        Task<mQuote> getQuoteInfo(string QrfId);

        Task<mQRFPrice> getQuotePriceInfo(string QrfId);

        Task<Bookings> getBookingInfo(string BookingNo);

        #endregion
    }
}
