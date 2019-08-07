using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VGER_WAPI.Repositories;
using VGER_WAPI.Helpers;
using Microsoft.Extensions.Options;
using VGER_WAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.Booking;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Booking")]
    public class BookingController : Controller
    {
        #region Private Variable Declaration
        private readonly IBookingRepository _bookingRepository;
        private readonly MongoContext _MongoContext = null;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public BookingController(IBookingRepository bookingRepository, IOptions<MongoSettings> settings, IMSDynamicsRepository mSDynamicsRepository)
        {
            _bookingRepository = bookingRepository;
            _MongoContext = new MongoContext(settings);
            _mSDynamicsRepository = mSDynamicsRepository;
        }

        #region mBooking Collection
        [Authorize]
        [HttpPost]
        [Route("GetBookingDetails")]
        public async Task<BookingSearchRes> GetBookingDetails([FromBody] BookingSearchReq request)
        {
            var response = new BookingSearchRes();
            try
            {
                response = await _bookingRepository.GetBookingDetails(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetBookingStatusList")]
        public BookingSearchRes GetBookingStatusList()
        {
            var response = new BookingSearchRes();
            try
            {
                response.BookingStatusList = _bookingRepository.GetBookingStatusList();
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetBookingRoomDetails")]
        public async Task<BookingSearchRes> GetBookingRoomDetails([FromBody] BookingSearchReq request)
        {
            var response = new BookingSearchRes();
            try
            {
                response = await _bookingRepository.GetBookingRoomDetails(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetBookingPositionPricingDetails")]
        public async Task<BookingSearchRes> GetBookingPositionPricingDetails([FromBody] BookingSearchReq request)
        {
            var response = new BookingSearchRes();
            try
            {
                response = await _bookingRepository.GetBookingPositionPricingDetails(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetBookingDocumentDetails")]
        public async Task<BookingDocumentGetRes> GetBookingDocumentDetails([FromBody] BookingDocumentGetReq request)
        {
            var response = new BookingDocumentGetRes();
            try
            {
                response = await _bookingRepository.GetBookingDocumentDetails(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("SetBookingDocumentDetails")]
        public async Task<BookingDocumentSetRes> SetBookingDocumentDetails([FromBody] BookingDocumentSetReq request)
        {
            var response = new BookingDocumentSetRes();
            try
            {
                response = await _bookingRepository.SetBookingDocumentDetails(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("SetBookingBackScriptDetails")]
        public async Task<ResponseStatus> SetBookingBackScriptDetails()
        {
            var response = new ResponseStatus();
            try
            {
                response = await _bookingRepository.SetBookingBackScriptDetails();
                response.Status = "Success";
                response.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }
        #endregion

        #region Booking Collection
        /// <summary>
        /// Get Search Booking Details by BookingSearchReq object and returns the booking details
        /// </summary>
        /// <param name="request">Request will contains Booking start end date,bookingid,AgentCode,Status,etc</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetSearchBookingDetails")]
        public async Task<BookingSearchRes> GetSearchBookingDetails([FromBody] BookingSearchReq request)
        {
            var response = new BookingSearchRes();
            try
            {
                response = await _bookingRepository.GetSearchBookingDetails(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Get Booking Details by BookingDetailReq param and returns the booking details
        /// </summary>
        /// <param name="request">Request will contains Booking Number</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetBookingDetailsByParam")]
        public async Task<BookingInfoRes> GetBookingDetailsByParam([FromBody] BookingDetailReq request)
        {
            var response = new BookingInfoRes();
            try
            {
                response = await _bookingRepository.GetBookingDetailsByParam(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Set booking Positions will UPSERT the positions details into Bookings collection
        /// </summary>
        /// <param name="request">Request will contains Position object and UserEmailId</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetBookingPositions")]
        public async Task<BookingPositionsSetRes> SetBookingPositions([FromBody] BookingPositionsSetReq request)
        {
            var response = new BookingPositionsSetRes();
            try
            {
                response = await _bookingRepository.SetBookingPositions(request); 
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// cancel booking Positions will cancel the position status to 'C' and if all positions are in 'C' status then booking status will update to 'C'
        /// Send cancel booking mail 
        /// call bridge service to update the booking position details into SQL
        /// </summary>
        /// <param name="request">PositionId and UserEmailId</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("CancelBookingPositions")]
        public async Task<BookingPositionsSetRes> CancelBookingPositions([FromBody] BookingCancelPositionSetReq request)
        {
            var response = new BookingPositionsSetRes();
            try
            {
                response = await _bookingRepository.CancelBookingPositions(request);
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        #endregion

        #region MS Dynamics Create/Update Booking in CRM

        /// <summary>
        /// Creating /Update Booking in MS Dynamic System against the provided BOOking No and the user
        /// </summary>
        /// <param >BookingNo and VoyagerUser</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("CreateUpdateBookingInCRM")]
        public async Task<ResponseStatus> CreateUpdateBookingInCRM([FromQuery]string BookingNo, string VoyagerUser)
        {
            var response = new ResponseStatus();
            try
            {
                response = await _mSDynamicsRepository.CreateUpdateBooking(BookingNo, VoyagerUser);
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