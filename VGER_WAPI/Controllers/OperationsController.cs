using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.Booking;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Operations")]
    public class OperationsController : Controller
    {
        private readonly IOperationsRepository _opsRepository;

        public OperationsController(IOperationsRepository opsRepository)
        {
            _opsRepository = opsRepository;
        }

        #region Ops Search & View by Booking
        /// <summary>
        /// Get Product details by Search Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetOpsBookingDetails")]
        public async Task<OpsBookingSearchRes> GetOpsBookingDetails([FromBody] OpsBookingSearchReq request)
        {
            var response = new OpsBookingSearchRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetBookingDetails(request);
                    if (result != null && result.Bookings.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.BookingTotalCount = result.BookingTotalCount;
                        response.Bookings = result.Bookings.ToList();
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

        [Authorize]
        [HttpPost]
        [Route("GetOpsBookingPaxDetails")]
        public async Task<BookingPaxDetailsGetResponse> GetOpsBookingPaxDetails([FromBody] BookingPaxDetailsGetRequest request)
        {
            var response = new BookingPaxDetailsGetResponse();
            if (!string.IsNullOrEmpty(request.BookingNumber))
            {
                try
                {
                    response = await _opsRepository.GetOpsBookingPaxDetails(request);
                }
                catch (Exception ex)
                {
                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "An Error Occurs :- " + ex.Message;
                }
            }
            else
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "Booking Number cannot be Null";

            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetPositionsFromBooking")]
        public async Task<PositionsFromBookingGetRes> GetPositionsFromBooking([FromBody] PositionsFromBookingGetReq request)
        {
            var response = new PositionsFromBookingGetRes();

            try
            {
                if (request != null)
                {
                    response = await _opsRepository.GetPositionsFromBooking(request);
                    if (response == null)
                    {
                        response.Response.Status = "Failure";
                        response.Response.ErrorMessage = "An error.";
                    }
                }
                else
                {
                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "An error occurs " + ex.Message;
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
        [Route("GetOpsBookingSummary")]
        public async Task<OpsBookingSummaryGetRes> GetOpsBookingSummary([FromBody] ProductSRPHotelGetReq request)
        {
            var response = new OpsBookingSummaryGetRes();
            try
            {
                if (request != null)
                {
                    response = await _opsRepository.GetOpsBookingSummary(request);
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
        #endregion

        #region RoomingList
        [Authorize]
        [HttpPost]
        [Route("GetRoomingDetails")]
        public async Task<BookingRoomingGetResponse> GetRoomingDetails([FromBody] BookingRoomingGetRequest request)
        {
            var response = new BookingRoomingGetResponse();
            try
            {
                response = await _opsRepository.GetRoomingDetails(request);
            }
            catch (Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetRoomingDetailsForHotels")]
        public async Task<BookingRoomHotelsGetRes> GetRoomingDetailsForHotels([FromBody] BookingRoomingGetRequest request)
        {
            var response = new BookingRoomHotelsGetRes();
            try
            {
                response = await _opsRepository.GetRoomingDetailsForHotels(request);
            }
            catch (Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "An Error Occurred due to :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("SetRoomingDetails")]
        public async Task<BookingRoomingSetResponse> SetRoomingDetails([FromBody] SetPassengerDetailsReq request)
        {
            var response = new BookingRoomingSetResponse();
            if (!string.IsNullOrEmpty(request.Booking_Number))
            {
                try
                {
                    response = await _opsRepository.SetRoomingDetails(request);
                }
                catch (Exception ex)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
                }
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Booking Number cannot be Null";

            }

            return response;
        }
        [Authorize]
        [HttpPost]
        [Route("CancelRoomingListUpdate")]
        public async Task<OpsBookingSetRes> CancelRoomingListUpdate(string BookingNumber)
        {
            OpsBookingSetRes response = new OpsBookingSetRes();
            try
            {
                response = await _opsRepository.CancelRoomingListUpdate(BookingNumber);
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage.Add("An Error Occurs :- " + ex.Message);
            }
            return response;
        }
        [Authorize]
        [HttpPost]
        [Route("SaveRoomingListPersonDetails")]
        public async Task<BookingRoomingSetResponse> SaveRoomingListPersonDetails([FromBody] SetPassengerDetailsReq request)
        {
            var response = new BookingRoomingSetResponse();
            if (!string.IsNullOrEmpty(request.Booking_Number))
            {
                try
                {
                    response = await _opsRepository.SaveRoomingListPersonDetails(request);
                }
                catch (Exception ex)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
                }
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Booking Number cannot be Null";

            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("UpdateBookingRoomsDataAsperExcel")]
        public async Task<BookingRoomsSetResponse> UpdateBookingRoomsDataAsperExcel([FromBody] BookingRoomsSetRequest request)
        {
            var response = new BookingRoomsSetResponse();
            if (!string.IsNullOrEmpty(request.BookingNumber))
            {
                try
                {
                    response = await _opsRepository.UpdateBookingRoomsDataAsperExcel(request);
                }
                catch (Exception ex)
                {
                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "An Error Occurs :- " + ex.Message;
                }
            }
            else
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "Booking Number cannot be Null";

            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetOpsBookingRoomsDetails")]
        public async Task<BookingRoomGetResponse> GetOpsBookingRoomsDetails([FromBody] BookingRoomsGetRequest request)
        {
            var response = new BookingRoomGetResponse();
            if (!string.IsNullOrEmpty(request.BookingNumber))
            {
                try
                {
                    response = await _opsRepository.GetOpsBookingRoomsDetails(request);
                }
                catch (Exception ex)
                {
                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "An Error Occurs :- " + ex.Message;
                }
            }
            else
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "Booking Number cannot be Null";

            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetQRFPackagePriceForRoomsDetails")]
        public async Task<QrfPackagePriceGetRes> GetQRFPackagePriceForRoomsDetails([FromBody] QrfPackagePriceGetReq request)
        {
            var response = new QrfPackagePriceGetRes();
            if (!string.IsNullOrEmpty(request.QrfId))
            {
                try
                {
                    response = await _opsRepository.GetQRFPackagePriceForRoomsDetails(request);
                }
                catch (Exception ex)
                {
                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "An Error Occurs :- " + ex.Message;
                }
            }
            else
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "Booking Number cannot be Null";

            }

            return response;
        }
        #endregion

        #region View Service Status->Itinerary
        /// <summary>
        /// Get Booking ItineraryDetails by BookingNo and optional param are Day, ServiceType, ItineraryStatus
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetBookingItineraryDetails")]
        public async Task<OpsBookingItineraryGetRes> GetBookingItineraryDetails([FromBody] OpsBookingItineraryGetReq request)
        {
            var response = new OpsBookingItineraryGetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetBookingItineraryDetails(request);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
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
        /// Get Position Type by BookingNo and Position Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPositionTypeByPositionId")]
        public async Task<OpsBookingItineraryGetRes> GetPositionTypeByPositionId([FromBody] OpsBookingItineraryGetReq request)
        {
            var response = new OpsBookingItineraryGetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetPositionTypeByPositionId(request);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
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

        #region Opeartion Common Service
        /// <summary>
        /// Get Operation Header Details by Passing BookingNo or Booking as object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetOperationHeaderDetails")]
        public async Task<OperationHeaderInfo> GetOperationHeaderDetails([FromBody] OpsHeaderGetReq request)
        {
            var response = new OperationHeaderInfo();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetOperationHeaderDetails(request);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Booking details can not be null.";
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
        /// Get Operation ProductType Details by Passing DayName,PositionId,ProductType and return the details of ProductType
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetOpsProductTypeDetails")]
        public async Task<OpsProductTypeGetRes> GetOpsProductTypeDetails([FromBody] OpsProductTypeGetReq request)
        {
            var response = new OpsProductTypeGetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetOpsProductTypeDetails(request);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "OpsProductTypeGetReq details can not be null.";
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
        ///Get ProductType of Position By Param Details by Passing PositionId,ProductType,BookingNumber and return the details of ProductType
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProdTypePositionByParam")]
        public async Task<OpsProductTypeGetRes> GetProdTypePositionByParam([FromBody] OpsProdTypePositionGetReq request)
        {
            var response = new OpsProductTypeGetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetProdTypePositionByParam(request);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "OpsProdTypePositionGetReq details can not be null.";
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
        ///Get PersonType of Product Range By Param Details by Passing ProductId, ProductCategory, ProductRangeId and return the details of PersonType
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPersonTypeByProductRange")]
        public async Task<OpsProdRangePersTypeGetRes> GetPersonTypeByProductRange([FromBody] OpsProdRangePersTypeGetReq request)
        {
            var response = new OpsProdRangePersTypeGetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetPersonTypeByProductRange(request);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An Error Occurred.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "OpsProdRangePersTypeGetReq details can not be null.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurred " + ex.Message;
            }
            return response;
        }

        #endregion

        #region OpsPositionFOC

        /// <summary>
        /// Method to Set Position FOC details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetBookingPositionFOC")]
        public async Task<OpsFOCSetRes> SetBookingPositionFOC([FromBody] OpsFOCSetReq request)
        {
            var response = new OpsFOCSetRes();
            try
            {
                response = await _opsRepository.SetBookingPositionFOC(request);
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

        #endregion

        #region Financials
        /// <summary>
        /// Method to get Financial details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetOpsFinancialDetails")]
        public async Task<OpsFinancialsGetRes> GetOpsFinancialDetails([FromBody] OpsFinancialsGetReq request)
        {
            var response = new OpsFinancialsGetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetOpsFinancialDetails(request);
                    if (result != null)
                    {
                        response = result;
                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "OpsProductTypeGetReq details can not be null.";
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

        #region SetBookingByWorkflow

        /// <summary>
        /// Method to Set Position details for Booking
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetBookingByWorkflow")]
        public async Task<OpsBookingSetRes> SetBookingByWorkflow([FromBody] OpsBookingSetReq request)
        {
            var response = new OpsBookingSetRes();
            try
            {
                if (request != null)
                {
                    if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                    {
                        if (request.OpsKeyValue?.Count > 0)
                        {
                            response = await _opsRepository.SetBookingByWorkflow(request);
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage.Add("Position details not found.");
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage.Add("Booking Number not found.");
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage.Add("OpsBookingSetReq cannot be null.");
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage.Add("An Error Occurs :- " + ex.Message);
            }

            return response;
        }

        #endregion

        #region Itinerary Builder

        /// <summary>
        /// Get Booking ItineraryDetails by BookingNo and optional param are Day, ServiceType, ItineraryStatus
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetItineraryBuilderDetails")]
        public async Task<OpsBookingItineraryGetRes> GetItineraryBuilderDetails([FromBody] OpsBookingItineraryGetReq request)
        {
            var response = new OpsBookingItineraryGetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetItineraryBuilderDetails(request);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
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
        /// Get Booking ItineraryDetails by BookingNo and optional param are Day, ServiceType, ItineraryStatus
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetItineraryBuilderPositionDetailById")]
        public async Task<OpsBookingItineraryGetRes> GetItineraryBuilderPositionDetailById([FromBody] OpsBookingItineraryGetReq request)
        {
            var response = new OpsBookingItineraryGetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.GetItineraryBuilderPositionDetailById(request);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
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
        /// Get Booking ItineraryDetails by BookingNo and optional param are Day, ServiceType, ItineraryStatus
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetRemarksForItineraryBuilderDetails")]
        public async Task<OpsBookingItinerarySetRes> SetRemarksForItineraryBuilderDetails([FromBody] OpsBookingItinerarySetReq request)
        {
            var response = new OpsBookingItinerarySetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.SetRemarksForItineraryBuilderDetails(request);
                    if (result != null)
                    {
                        response = result;
                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
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
        /// Get Booking ItineraryDetails by BookingNo and optional param are Day, ServiceType, ItineraryStatus
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetItineraryBuilderDetails")]
        public async Task<OpsBookingItinerarySetRes> SetItineraryBuilderDetails([FromBody] OpsBookingItinerarySetReq request)
        {
            var response = new OpsBookingItinerarySetRes();
            try
            {
                if (request != null)
                {
                    var result = await _opsRepository.SetItineraryBuilderDetails(request);
                    if (result != null)
                    {
                        response = result;
                        response.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Ann Error Occurs.";
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

        #region Payment Schedule
        /// <summary>
        /// Method to Get PaymentSchedule For Position
        /// </summary>
        /// <param name="request">request contains Booking Number and PositionId</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPositionPaymentSchedule")]
        public async Task<PaymentScheduleGetRes> GetPositionPaymentSchedule([FromBody] PaymentScheduleGetReq request)
        {
            var response = new PaymentScheduleGetRes();
            try
            {
                if (request != null)
                {
                    response = await _opsRepository.GetPositionPaymentSchedule(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "PaymentScheduleGet cannot be null.";
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
    }
}