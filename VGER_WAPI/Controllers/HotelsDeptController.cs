using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/HotelsDept")]
    public class HotelsDeptController : Controller
    {
        private readonly IHotelsDeptRepository _hotelsRepository;

        public HotelsDeptController(IHotelsDeptRepository hotelsRepository)
        {
            _hotelsRepository = hotelsRepository;
        }

        #region Hotels Search & View by Booking
        /// <summary>
        /// Get Product details by Search Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetHotelsByBookingDetails")]
        public async Task<HotelsDeptSearchRes> GetHotelsByBookingDetails([FromBody] BookingSearchReq request)
        {
            var response = new HotelsDeptSearchRes();
            try
            {
                if (request != null)
                {
                    var result = await _hotelsRepository.GetHotelsByBookingDetails(request);
                    if (result != null && result.BookingsDetails.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
						response.HotelsTotalCount = result.HotelsTotalCount;
                        response.BookingsDetails = result.BookingsDetails.ToList();
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
                    response.ResponseStatus.ErrorMessage = "Booking details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Product Hotel Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductHotelDetails")]
        public async Task<HotelsByBookingGetRes> GetProductHotelDetails([FromBody] ProductSRPHotelGetReq request)
        {
            var response = new HotelsByBookingGetRes();
            try
            {
                if (request != null)
                {
                    response = await _hotelsRepository.GetProductHotelDetails(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Product Hotel Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetAlternateServicesByBooking")]
        public async Task<HotelAlternateServicesGetRes> GetAlternateServicesByBooking([FromBody] HotelAlternateServicesGetReq request)
        {
            var response = new HotelAlternateServicesGetRes();
            try
            {
                if (request != null)
                {
                    response = await _hotelsRepository.GetAlternateServicesByBooking(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Send Hotel Reservation Request Email to all alternate hotels saved in Bookings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SendHotelReservationRequestEmail")]
        public async Task<HotelReservationEmailRes> SendHotelReservationRequestEmail([FromBody] HotelReservationRequestEmail request)
        {
            var response = new HotelReservationEmailRes();
            try
            {
                if (request != null)
                {
                    response = await _hotelsRepository.SendHotelReservationRequestEmail(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error occurred.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Hotel Availability Request Details against alternate hotels saved in Bookings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("GetHotelAvailabilityRequestDetails")]
        public async Task<AvailabilityRequestDetailsGetRes> GetHotelAvailabilityRequestDetails([FromBody] AvailabilityRequestDetailsGetReq request)
        {
            var response = new AvailabilityRequestDetailsGetRes();
            try
            {
                if (request != null)
                {
                    response = await _hotelsRepository.GetHotelAvailabilityRequestDetails(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error occurred.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Update Hotel Availability Request against alternate hotels saved in Bookings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateHotelAvailabilityRequest")]
        public async Task<HotelReservationEmailRes> UpdateHotelAvailabilityRequest([FromBody] AvailabilityRequestDetailsSetReq request)
        {
            var response = new HotelReservationEmailRes();
            try
            {
                if (request != null && request.UpdateReqDetails != null && request.AltSvcRoomsAndPrices != null &&
                    !string.IsNullOrEmpty(request.BookingNumber) && !string.IsNullOrEmpty(request.PositionId) && !string.IsNullOrEmpty(request.AltSvcId))
                {
                    response = await _hotelsRepository.UpdateHotelAvailabilityRequest(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error occurred.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Booking details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Budget Supplement based on ALternate Hotel Rate provided
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetBudgetSupplement")]
        public async Task<BudgetSupplementGetRes> GetBudgetSupplement([FromBody] BudgetSupplementGetReq request)
        {
            var response = new BudgetSupplementGetRes();
            try
            {
                if (request != null)
                {
                    response = await _hotelsRepository.GetBudgetSupplement(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error occurred.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Set Budget Supplement for ALternate Hotel
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("SetBudgetSupplement")]
        public async Task<CommonResponse> SetBudgetSupplement([FromBody] BudgetSupplementSetReq request)
        {
            var response = new CommonResponse();
            try
            {
                if (request != null && request.BudgetSupplements != null &&
                    !string.IsNullOrEmpty(request.BookingNumber) && !string.IsNullOrEmpty(request.PositionId) && !string.IsNullOrEmpty(request.AltSvcId))
                {
                    response = await _hotelsRepository.SetBudgetSupplement(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error occurred.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Booking details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Booking Details to switch between Hotels from Bookings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetHotelActivationDetails")]
        public async Task<ActivateHotelDetailsGetRes> GetHotelActivationDetails([FromBody] AvailabilityRequestDetailsGetReq request)
        {
            var response = new ActivateHotelDetailsGetRes();
            try
            {
                if (request != null)
                {
                    response = await _hotelsRepository.GetHotelActivationDetails(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error occurred.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Update Booking Details to switch between Hotels from Bookings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateHotelActivationDetails")]
        public async Task<HotelReservationEmailRes> UpdateHotelActivationDetails([FromBody] AvailabilityRequestDetailsGetReq request)
        {
            var response = new HotelReservationEmailRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.BookingNumber) && !string.IsNullOrEmpty(request.PositionId) && !string.IsNullOrEmpty(request.AltSvcId))
                {
                    response = await _hotelsRepository.UpdateHotelActivationDetails(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error occurred.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Booking details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        #endregion
    }
}