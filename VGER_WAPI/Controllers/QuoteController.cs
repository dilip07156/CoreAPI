using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Quote")]
    public class QuoteController : Controller
    {
        #region Private Variable Declaration
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQRFSummaryRepository _qRFSummaryRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IConfiguration _configuration;
        private readonly IPositionRepository _positionRepository;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public QuoteController(IConfiguration configuration, IQuoteRepository quoteRepository, IQRFSummaryRepository qRFSummaryRepository, IEmailRepository emailRepository,IPositionRepository positionRepository, IMSDynamicsRepository mSDynamicsRepository)
        {
            _configuration = configuration;
            _quoteRepository = quoteRepository;
            _qRFSummaryRepository = qRFSummaryRepository;
            _emailRepository = emailRepository;
            _positionRepository = positionRepository;
            _mSDynamicsRepository = mSDynamicsRepository;
        }

        #region Agent
        /// <summary>
        /// Get Agent Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetAgentCompanies")]
        public AgentCompanyRes GetAgentCompanies([FromBody] AgentCompanyReq request)
        {
            var response = new AgentCompanyRes();
            try
            {
                if (request != null)
                {
                    List<AgentProperties> result = _quoteRepository.GetAgentCompanies(request);
                    if (result != null && result.Count() > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.AgentProperties = result.Distinct().OrderBy(d => d.Name).ToList();
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Agent Name can not be Null.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Agent Details from mCompanies
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetAgentCompaniesfrommCompanies")]
        public AgentCompanyRes GetAgentCompaniesfrommCompanies([FromBody] AgentCompanyReq request)
        {
            var response = new AgentCompanyRes();
            try
            {
                if (request != null)
                {
                    IQueryable<AgentProperties> result = (IQueryable<AgentProperties>)_quoteRepository.GetAgentCompaniesfrommCompanies(request);
                    if (result != null && result.Count() > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.AgentProperties = result.OrderBy(d => d.Name).ToList();
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Agent Name can not be Null.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Supplier Details from mCompanies
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetSuppliersfrommCompanies")]
        public AgentCompanyRes GetSuppliersfrommCompanies([FromBody] AgentCompanyReq request)
        {
            var response = new AgentCompanyRes();
            try
            {
                if (request != null)
                {
                    IQueryable<AgentProperties> result = (IQueryable<AgentProperties>)_quoteRepository.GetSuppliersfrommCompanies(request);
                    if (result != null && result.Count() > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.AgentProperties = result.OrderBy(d => d.Name).ToList();
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Agent Name can not be Null.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get System Company Details
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetCompanyDetails")]
        public CompanyDetailsRes GetCompanyDetails()
        {
            var response = new CompanyDetailsRes();
            try
            {
                response = _quoteRepository.GetCompanyDetails();
                if (response != null)
                {
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "No Records Found.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occured : " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Contact details by Agent Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetContactsForAgentCompany")]
        public AgentContactRes GetContactsForAgentCompany([FromBody] AgentContactReq request)
        {
            var response = new AgentContactRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.Company_Id))
                {
                    IQueryable<ContactProperties> result = (IQueryable<ContactProperties>)_quoteRepository.GetContactsForAgentCompany(request);
                    if (result != null && result.Count() > 0)
                    {
                        response.ContactProperties = result.OrderBy(a => a.FullName).ToList();
                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Contact Name can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Check if Duplicate TourName already exists
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("CheckDuplicateQRFTourName")]
        public AgentContactRes CheckDuplicateQRFTourName([FromBody] AgentContactReq request)
        {
            var response = new AgentContactRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.Company_Id))
                {
                    string result = _quoteRepository.CheckDuplicateQRFTourName(request);
                    if (result != null && result.Count() > 0)
                    {
                        response.TourNameFlag = result;
                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Contact Name can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Contact details by Agent Id and Contact Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetContactDetailsByAgentAndContactID")]
        public async Task<AgentContactDetailsRes> GetContactDetailsByAgentAndContactID([FromBody] AgentContactDetailsReq request)
        {
            var response = new AgentContactDetailsRes();
            try
            {
                if (request != null)
                {
                    AgentContactDetailsProperties result = await _quoteRepository.GetContactDetailsByAgentAndContactID(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";
                    response.AgentContactDetailsProperties = result;
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Contact Details Can not be Null";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Insert /Update Quote Agent details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("InsertUpdateQRFAgentDetails")]
        public async Task<QuoteAgentSetRes> InsertUpdateQRFAgentDetails([FromBody] QUOTEAgentSetReq request)
        {
            var response = new QuoteAgentSetRes();
            try
            {
                if (request != null)
                {
                    string result = await _quoteRepository.InsertUpdateQRFAgentDetails(request);
                    response.QRFID = result;
                    response.ResponseStatus.Status = "Success";

                    #region Integration Service (MSDynamics)

                    //Save Opportunity to CRM system
                    if (!string.IsNullOrEmpty(result))
                    {
                        //Task.Run(async () => _mSDynamicsRepository.CreateOpportunity(request, result).Result);
                        Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(result, request.ckLoginUser_Id).Result);
                    } 

                    #endregion
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get QRF Agent details by serach param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetQRFAgentDetailsBySearchCriteria")]
        public async Task<QuoteSearchRes> GetQRFAgentDetailsBySearchCriteria([FromBody] QuoteSearchReq request)
        {
            var response = new QuoteSearchRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.CurrentPipeline))
                {
                    var result = await _quoteRepository.GetQRFAgentDetailsBySearchCriteria(request);
                    if (result != null && result.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.QuoteSearchDetails = result.ToList();
                        response.TotalCount = result.ToList().Count;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Pipeline details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Quote Agent details by QRF ID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetQRFAgentDetailsByQRFID")]
        public async Task<QuoteAgentGetRes> GetQRFAgentDetailsByQRFID([FromBody] QuoteAgentGetReq request)
        {
            var response = new QuoteAgentGetRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    QuoteAgentGetProperties result = await _quoteRepository.GetQRFAgentDetailsByQRFID(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";
                    response.QuoteAgentGetProperties = result;
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Zero.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }

            return response;
        }

		/// <summary>
		/// Get Quote Agent details by QRF ID
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Authorize]
		[HttpPost]
		[Route("GetDivision")]
		public async Task<QuoteAgentGetRes> GetDivision([FromBody] QuoteSearchReq request)
		{
			var response = new QuoteAgentGetRes();
			try
			{
				if (request != null && !string.IsNullOrEmpty(request.UserName))
				{
                    DivisionGetRes result = new DivisionGetRes();
					result = await _quoteRepository.GetDivision(request);
					response.ResponseStatus.Status = "Success";
					response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";
					response.QuoteAgentGetProperties.Division = result.DivisionList;
                    response.CompanyDivision = result.CompanyDivision;
                }
				else
				{
					response.ResponseStatus.Status = "Failure";
					response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Zero.";
				}
			}
			catch (Exception ex)
			{
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
			}

			return response;
		}
		#endregion

		#region FollowUp Quote
		///// <summary>
		///// Method to get followup for requested QRF_Id
		///// </summary>
		///// <param name="request"></param>
		///// <returns></returns>
		//[HttpPost]
		//[Authorize]
		//[Route("GetFollowUpForQRF_Id")]
		//public QrfFollowUpResponse GetFollowUpForQRF_Id([FromBody] QrfFollowUpRequest request)
		//{
		//    var response = new QrfFollowUpResponse();
		//    if (!string.IsNullOrWhiteSpace(request.QRFID))
		//    {
		//        if (!string.IsNullOrWhiteSpace(request.FollowUp_Id))
		//        {
		//            var result = _quoteRepository.GetFollowUpForFollowUp_Id(request);
		//            var FollowUp = new FollowUpItem();
		//            if (result.ToList()[0].Count > 0)
		//            {
		//                foreach (var x in result)
		//                {
		//                    foreach (var y in x)
		//                    {
		//                        FollowUp = y;
		//                    }
		//                }
		//                response.FollowUpItems.Add(FollowUp);
		//                response.QRFID = request.QRFID;
		//                response.Status = "Success";
		//                return response;
		//            }
		//            else
		//            {
		//                response.Status = "Invalid FollowUp_Id";
		//                return response;
		//            }
		//        }
		//        else
		//        {
		//            var results = _quoteRepository.GetFollowUpForQRF_Id(request);
		//            if (results.ToList()[0].Count > 0)
		//            {
		//                foreach (var x in results)
		//                {
		//                    foreach (var y in x)
		//                    {
		//                        response.FollowUpItems.Add(y);
		//                    }
		//                }
		//                response.QRFID = request.QRFID;
		//                response.Status = "Success";
		//                return response;
		//            }
		//            else
		//            {
		//                response.Status = "Invalid QRF_Id";
		//                return response;
		//            }
		//        }
		//    }
		//    else
		//    {
		//        response.Status = "Failure";
		//        return response;
		//    }
		//}

		///// <summary>
		///// Method to set followup for requested QRF_Id
		///// </summary>
		///// <param name="request"></param>
		///// <returns></returns>
		//[HttpPost]
		//[Authorize]
		//[Route("SetFollowUpForQRF_Id")]
		//public QrfFollowUpSetResponse SetFollowUpForQRF_Id([FromBody] QrfFollowUpSetRequest request)
		//{
		//    var response = new QrfFollowUpSetResponse();
		//    if (!string.IsNullOrWhiteSpace(request.QRFID))
		//    {
		//        var res = _quoteRepository.SetFollowUpForQRF_Id(request);
		//        res.Wait();
		//        if (res.Result == true)
		//        {
		//            response.Status = "Success";
		//        }
		//        else
		//        {
		//            response.Status = "Failure";
		//        }
		//        return response;
		//    }
		//    else
		//    {
		//        response.Status = "Primary key is blank";
		//        return response;
		//    }
		//}
		#endregion

		#region Departures
		/// <summary>
		/// Insert Update Departures for a QRF
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
        [Authorize]
        [Route("SetDepartureDatesForQRF_Id")]
        public DepartureDateSetResponse SetDepartureDatesForQRF_Id([FromBody] DepartureDateSetRequest request)
        {
            var response = new DepartureDateSetResponse();
            if (!string.IsNullOrEmpty(request.QRFID))
            {
                var res = _quoteRepository.SetDepartureDatesForQRF_Id(request);
                res.Wait();
                response.Status = res.Result.Status;
                if (!string.IsNullOrEmpty(res.Result.Status) && res.Result.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserId).Result);

                }
                return response;
            }
            else
            {
                response.Status = "Primary key is blank";
                return response;
            }
        }

        /// <summary>
        /// Get Departures for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetDepartureDatesForQRF_Id")]
        public DepartureDateGetResponse GetDepartureDatesForQRF_Id([FromBody] DepartureDateGetRequest request)
        {
            var response = new DepartureDateGetResponse();
            if (request != null)
            {
                var res = _quoteRepository.GetDepartureDatesForQRF_Id(request);
                return res;
            }
            else
            {
                response.Status = "Primary key is blank";
                return response;
            }
        }

        #endregion

        #region PaxSlabDetails
        /// <summary>
        /// Get PaxSlabDetails for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetPaxSlabDetailsForQRF_Id")]
        public PaxGetResponse GetPaxSlabDetailsForQRF_Id([FromBody] PaxGetRequest request)
        {
            var response = new PaxGetResponse();
            if (request != null)
            {
                var res = _quoteRepository.GetPaxSlabDetailsForQRF_Id(request);
                return res;
            }
            else
            {
                response.Status = "Primary key is blank";
                return response;
            }
        }

        /// <summary>
        /// Insert update PaxSlabDetails for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("SetPaxSlabDetailsForQRF_Id")]
        public PaxSetResponse SetPaxSlabDetailsForQRF_Id([FromBody] PaxSetRequest request)
        {
            var response = new PaxSetResponse();
            if (!string.IsNullOrEmpty(request.QRFID))
            {
                var res = _quoteRepository.SetPaxSlabDetailsForQRF_Id(request);
                res.Wait();
                response.Status = res.Result.Status;
                if (!string.IsNullOrEmpty(res.Result.Status) && res.Result.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserId).Result);
                }
                return response;
            }
            else
            {
                response.Status = "Primary key is blank";
                return response;
            }
        }

        #endregion

        #region FOCDetails
        /// <summary>
        /// Get FOCDetails for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetFOCDetailsForQRF_Id")]
        public FOCGetResponse GetFOCDetailsForQRF_Id([FromBody] PaxGetRequest request)
        {
            var response = new FOCGetResponse();
            if (request != null)
            {
                var res = _quoteRepository.GetFOCDetailsForQRF_Id(request);
                return res;
            }
            else
            {
                response.Status = "Primary key is blank";
                return response;
            }
        }

        /// <summary>
        /// Insert update FOCDetails for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("SetFOCDetailsForQRF_Id")]
        public PaxSetResponse SetFOCDetailsForQRF_Id([FromBody] FOCSetRequest request)
        {
            var response = new PaxSetResponse();
            if (!string.IsNullOrEmpty(request.QRFID))
            {
                var res = _quoteRepository.SetFOCDetailsForQRF_Id(request);
                res.Wait();
                response.Status = res.Result.Status;
                if (!string.IsNullOrEmpty(res.Result.Status) && res.Result.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserId).Result);
                }
                return response;
            }
            else
            {
                response.Status = "Primary key is blank";
                return response;
            }
        }

        #endregion

        #region Routing
        /// <summary>
        /// Get QRF Route Details By QRFID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetQRFRouteDetailsByQRFID")]
        public async Task<RoutingGetRes> GetQRFRouteDetailsByQRFID([FromBody] RoutingGetReq request)
        {
            var response = new RoutingGetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    List<RoutingInfo> result = await _quoteRepository.GetQRFRouteDetailsByQRFID(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

                    response.RoutingInfo = result;
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Zero.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Insert /Update Quote Route details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("InsertUpdateQRFRouteDetails")]
        public async Task<RoutingSetRes> InsertUpdateQRFRouteDetails([FromBody] RoutingSetReq request)
        {
            var response = new RoutingSetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null)
                {
                    string result = await _quoteRepository.InsertUpdateQRFRouteDetails(request);
                    if(result == "1" && request.IsSetPrefHotels)
                    {
                        bool resultPosition = await _quoteRepository.AddHotels(request.RoutingInfo[0].CreateUser,request.QRFID,request.IsOverwriteExtPos);

                        if (resultPosition)
                        {
                            bool result2 = await _positionRepository.SetAllPriceFOCByQRFID(request.QRFID, request.RoutingInfo[0].CreateUser);
                        }
                    }
                    response.ResponseStatus.Status = result == "1" ? "Success" : "Failure";
                    response.ResponseStatus.ErrorMessage = result != "1" ? result : "";
                    if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                    {
                        //Task.Run(async () => _mSDynamicsRepository.CreateOpportunity(request, result).Result);
                        Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserId).Result);
                        
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        #endregion

        #region Routing Days
        /// <summary>
        /// Insert /Update Routing Days details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("InsertUpdateQRFRoutingDays")]
        public async Task<RoutingDaysGetRes> InsertUpdateQRFRoutingDays([FromBody] RoutingDaysSetReq request)
        {
            var response = new RoutingDaysGetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null)
                {
                    response = await _quoteRepository.InsertUpdateQRFRoutingDays(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Routing Days details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetQRFRoutingDays")]
        public async Task<RoutingDaysGetRes> GetQRFRoutingDays([FromBody] RoutingDaysGetReq request)
        {
            var response = new RoutingDaysGetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null)
                {
                    response = await _quoteRepository.GetQRFRoutingDays(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }
        #endregion        

        #region Margins
        /// <summary>
        /// Get QRF Margin Details By QRFID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost]
        [Authorize]
        [HttpPost]
        [Route("GetQRFMarginDetailsByQRFID")]
        public async Task<MarginGetRes> GetQRFMarginDetailsByQRFID([FromBody] MarginGetReq request)
        {
            var response = new MarginGetRes();
            try
            {
                response.QRFID = request.QRFID;
                Margins result = await _quoteRepository.GetQRFMarginDetailsByQRFID(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";
                response.Margins = result;

            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
                response.QRFID = request.QRFID;
            }

            return response;
        }

        /// <summary>
        /// Insert /Update Quote Margin details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("InsertUpdateQRFMarginDetails")]
        public async Task<MarginSetRes> InsertUpdateQRFMarginDetails([FromBody] MarginSetReq request)
        {
            var response = new MarginSetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null)
                {
                    string result = await _quoteRepository.InsertUpdateQRFMarginDetails(request);

                    if (request.IsCostingMargin && result == "1")
                    {
                        await _qRFSummaryRepository.SaveQRFPrice("Commercial", "Margin Changed", request.QRFID, request.Margins.EditUser);
                        response.ResponseStatus.Status = result == "1" ? "Success" : "Failure";
                        response.ResponseStatus.ErrorMessage = result != "1" ? result : "Margin details saved successfully and Cost Recalculated.";
                    }
                    else
                    {
                        response.ResponseStatus.Status = result == "1" ? "Success" : "Failure";
                        response.ResponseStatus.ErrorMessage = result != "1" ? result : "Margin details saved successfully.";
                    }
                    if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                    {
                        //Task.Run(async () => _mSDynamicsRepository.CreateOpportunity(request, result).Result);
                        Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserId).Result);

                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Active Currency List
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [HttpPost]
        [Route("GetActiveCurrencyList")]
        public CurrencyGetRes GetActiveCurrencyList([FromBody] CurrencyGetReq request)
        {
            var response = new CurrencyGetRes();
            try
            {
                if (request != null)
                {
                    var result = _quoteRepository.GetActiveCurrencyList(request);
                    response.Currency = result;
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Zero.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }
        #endregion

        #region FOC

        //[Authorize]
        //[HttpPost]
        //[Route("GetQRFFoc")]
        //public async Task<QRFFocGetRes> GetQRFFoc([FromBody] QRFFocGetReq request)
        //{
        //    var response = new QRFFocGetRes();
        //    try
        //    {
        //        if (request != null && !string.IsNullOrEmpty(request.QRFID))
        //        {
        //            List<QRFFocInfo> result = await _quoteRepository.GetQRFFoc(request);
        //            response.ResponseStatus.Status = "Success";
        //            response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

        //            response.QRFFocInfo = result;
        //        }
        //        else
        //        {
        //            response.ResponseStatus.Status = "Failure";
        //            response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Zero.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.ResponseStatus.Status = "Failure";
        //        response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
        //    }

        //    return response;
        //}

        #endregion

        #region Tour Entities
        /// <summary>
        /// Set Tour Entities details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetTourEntities")]
        public async Task<TourEntitiesSetRes> SetTourEntities([FromBody] TourEntitiesSetReq request)
        {
            var response = new TourEntitiesSetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null)
                {
                    response = await _quoteRepository.SetTourEntities(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserID))
                {
                    //Task.Run(async () => _mSDynamicsRepository.CreateOpportunity(request, result).Result);
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserID).Result);

                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Tour Entities details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetTourEntities")]
        public async Task<TourEntitiesGetRes> GetTourEntities([FromBody] TourEntitiesGetReq request)
        {
            var response = new TourEntitiesGetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null)
                {
                    response = await _quoteRepository.GetTourEntities(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }
        #endregion

        #region Meal
        /// <summary>
        /// Set Meal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetMeals")]
        public async Task<MealSetRes> SetMeals([FromBody] MealSetReq request)
        {
            var response = new MealSetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null)
                {
                    response = await _quoteRepository.SetMeals(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserId).Result);
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Meal details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetMeals")]
        public async Task<MealGetRes> GetMeals([FromBody] MealGetReq request)
        {
            var response = new MealGetRes();
            try
            {
                response.QRFID = request.QRFID;
                if (request != null)
                {
                    response = await _quoteRepository.GetMeals(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }
        #endregion

        #region FollowUp
        /// <summary>
        /// Insert Update FollowUp for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("SetFollowUpForQRF")]
        public FollowUpSetRes SetFollowUpForQRF([FromBody] FollowUpSetReq request)
        {
            var response = new FollowUpSetRes();
            if (!string.IsNullOrEmpty(request.QRFID))
            {
                var res = _quoteRepository.SetFollowUpForQRF(request);
                res.Wait();
                return res.Result;
            }
            else
            {
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = "Invalid Request";
                return response;
            }
        }

        /// <summary>
        /// Get Follow Up for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetFollowUpForQRF")]
        public FollowUpGetRes GetFollowUpForQRF([FromBody] FollowUpGetReq request)
        {
            var response = new FollowUpGetRes();
            if (request != null)
            {
                var res = _quoteRepository.GetFollowUpForQRF(request);
                return res;
            }
            else
            {
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = "Invalid Request";
                return response;
            }
        }

        /// <summary>
        /// Get Follow Up Master for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetFollowUpMasterData")]
        public FollowUpMasterGetRes GetFollowUpMasterData([FromBody] FollowUpGetReq request)
        {
            var response = new FollowUpMasterGetRes();
            if (request != null)
            {
                var res = _quoteRepository.GetFollowUpMasterData(request);
                return res;
            }
            else
            {
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = "Invalid Request";
                return response;
            }
        }

        /// <summary>
        /// Send Mail for Quote FollowUp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("SendQuoteFollowUpMail")]
        public EmailGetRes SendQuoteFollowUpMail([FromBody] EmailGetReq request)
        {
            var response = new EmailGetRes();
            if (!string.IsNullOrEmpty(request.QrfId) && !string.IsNullOrEmpty(request.FollowUpId))
            {
                request.DocumentType = DocType.QUOTEFOLLOWUP;
                var responseStatusMail = _emailRepository.GenerateEmail(request).Result;
                if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                {
                    responseStatusMail.ResponseStatus = new ResponseStatus();
                    responseStatusMail.ResponseStatus.Status = "Error";
                    responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent.";
                }
                return responseStatusMail;
            }
            else
            {
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = "Invalid Request";
                return response;
            }
        }

        #endregion

        /// <summary>
        /// Get Linked QRFs by QRF ID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetLinkedQRFs")]
        public async Task<LinkedQRFsGetRes> GetLinkedQRFs([FromBody] LinkedQRFsGetReq request)
        {
            var response = new LinkedQRFsGetRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    var result = await _quoteRepository.GetLinkedQRFs(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";
                    response.LinkedQRFsDataList = result;
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Empty.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Set Pipeline status of Quote to Rejected
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("SetQuoteRejectOpportunity")]
        public CommonResponse SetQuoteRejectOpportunity([FromBody] QuoteRejectOpportunityReq request)
        {
            var response = new CommonResponse();
            if (!string.IsNullOrEmpty(request.QRFID))
            {
                var res = _quoteRepository.SetQuoteRejectOpportunity(request);
                res.Wait();
                response = res.Result;
                if (!string.IsNullOrEmpty(request.QRFID) && response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => {
                        try
                        {
                            var returnResult = _mSDynamicsRepository.RejectOpportunityInfo(request.QRFID, request.VoyagerUserId).Result;
                        }
                        catch (Exception ex)
                        {
                            response.ResponseStatus.Status = "Error";
                            response.ResponseStatus.ErrorMessage = "While Rejecting Booking Opportunity in CRM system. \n" + ex.Message;
                        }
                    });
                }

                return response;
            }
            else
            {
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = "Invalid Request";
                return response;
            }
        }

        #region 3rd party Opportunity status
        /// <summary>
        /// Set Pipeline status of Quote to Rejected for 3rd party with reason
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("SetPartnerQuoteRejectOpportunity")]
        public OpportunityPartnerRes SetPartnerQuoteRejectOpportunity([FromBody] ManageOpportunityReq request)
        {
            var response = new OpportunityPartnerRes();
            if (!string.IsNullOrEmpty(request.OpportunityInfo.OpportunityId))
            {
                var res = _quoteRepository.SetPartnerQuoteRejectOpportunity(request);
                res.Wait();
                return res.Result;
            }
            else
            {
                response.ResponseStatus.Status = "Failed";
                response.ResponseStatus.ErrorMessage = "Invalid Request";
                return response;
            }
        }
        #endregion

        #region 3rd party search Quote

        /// <summary>
        /// GetPartnerQuoteDetails used for getting fetch QRFID VGER-mongodb (mQuote) based on "PartnerEntityCode" and "Application" provided by any 3rd party
        /// </summary>
        /// <param name="request">QuoteThirdPartyGetReq</param>
        /// <returns>
        /// Fetch QRFID from VGER-mongodb (mQuote)
        /// </returns>
        [Authorize]
        [HttpPost]
        [Route("GetPartnerQuoteDetails")]
        public async Task<QuoteThirdPartyGetRes> GetPartnerQuoteDetails([FromBody] QuoteThirdPartyGetReq request)
        {
            var response = new QuoteThirdPartyGetRes();
            try
            {
                response = await _quoteRepository.GetPartnerQuoteDetails(request);
                response.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(response.QRFID) ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        #endregion

        #region Copy Quote
        /// <summary>
        /// Get QRF Data For Copy Quote by QRF ID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetQRFDataForCopyQuote")]
        public async Task<GetQRFForCopyQuoteRes> GetQRFDataForCopyQuote([FromBody] QuoteAgentGetReq request)
        {
            var response = new GetQRFForCopyQuoteRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _qRFSummaryRepository.GetQRFDataForCopyQuote(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Empty.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Create new Quote from existing Quote by QRF ID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetCopyQuote")]
        public async Task<SetCopyQuoteRes> SetCopyQuote([FromBody] SetCopyQuoteReq request)
        {
            var response = new SetCopyQuoteRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    if (request.CopyQuoteDepartures != null && request.CopyQuoteDepartures.Count>0)
                    {
                        response = await _qRFSummaryRepository.SetCopyQuote(request);

                        if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(response.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                        {
                            Task.Run(() => _mSDynamicsRepository.CreateOpportunity(response.QRFID, request.QRFID, request.VoyagerUserId).Result);
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "No Departures found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Empty.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }

            return response;
        }
        #endregion

        #region Update ValidForAcceptance field in mQuote and mQRFPrice collection
        [Authorize]
        [HttpPost]
        [Route("UpdateValidForAcceptance")]
        public async Task<ResponseStatus> UpdateValidForAcceptance([FromBody] QuoteGetReq req)
        {
            var response = new ResponseStatus();
            try
            {
                if (!string.IsNullOrEmpty(req.QRFID))
                {
                    response = await _quoteRepository.UpdateValidForAcceptance(req);
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "QRFID can not be Blank/Null";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }
        #endregion
    }
}