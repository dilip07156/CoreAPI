using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_DISTRIBUTION.Repositories;
using VGER_WAPI_CLASSES;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace VGER_DISTRIBUTION.Controllers
{
    [Produces("application/json")]
    [Route("api/Booking")]
    public class BookingDataController : Controller //VoyagerController
    {
        #region Private Variable Declaration
        private readonly IConfiguration _configuration;
        private readonly IBookingRepository _bookingRepository;
        #endregion

        public BookingDataController(IBookingRepository bookingRepository, IConfiguration configuration) ////: base(configuration)
        {
            _bookingRepository = bookingRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Get Booking List
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetBookings")]
        [ProducesResponseType(typeof(BookingSearchRes), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBookings([FromBody] BookingSearchReq request)
        {
            var response = new BookingSearchRes();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (request != null)
                    {

                        var userdetails = Helpers.CreateToken.ReadToken(HttpContext);

                        var result = await _bookingRepository.GetBookings(request, userdetails);
                        if (result != null && result.Bookings.Count > 0)
                        {
                            response.ResponseStatus.Status = "Success";
                            response.Bookings = result.Bookings;
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "No Records Found.";
                            return NotFound(response.ResponseStatus);
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Booking details can not be blank.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            return Ok(response);
        }

        /// <summary>
        /// Get Booking Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetBookingDetail")]
        [ProducesResponseType(typeof(BookingDetailRes), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBookingDetail([FromBody] BookingDetailReq request)
        {
            var response = new BookingDetailRes();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (request != null)
                    {
                        var userdetails = Helpers.CreateToken.ReadToken(HttpContext);

                        if (!string.IsNullOrWhiteSpace(request.BookingReference))
                        {
                            if (request.BookingReference.Length == 6)
                            {
                                var result = await _bookingRepository.GetBookingDetail(request, userdetails);
                                if (result != null)
                                {
                                    if (result.BookingReference == null)
                                    {
                                        response.ResponseStatus.Status = "Failure";
                                        response.ResponseStatus.ErrorMessage = "User is not authorised to acess the booking " + request.BookingReference;
                                        //return NotFound(response.ResponseStatus);
                                        return Unauthorized();
                                    }
                                    else
                                    {
                                        response.ResponseStatus.Status = "Success";
                                        response.Booking = result;
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Failure";
                                    response.ResponseStatus.ErrorMessage = "User is not authorised to update the booking " + request.BookingReference;
                                    return NotFound(response.ResponseStatus);
                                }
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "InValid Booking Reference";
                                return BadRequest(response.ResponseStatus);
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Booking Reference cannot be blank";
                            return BadRequest(response.ResponseStatus);
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Booking Reference Number can not be blank.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            return Ok(response);
        }

        /// <summary>
        /// Update Operational Details
        /// </summary>
        /// <param name="requestbody"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateOperationDetails")]
        [ProducesResponseType(typeof(UpdateOperationDetails_RS), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOperationDetails([FromBody] UpdateOperationDetails_RQ requestbody)
        {
            var response = new UpdateOperationDetails_RS();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (requestbody != null)
                    {
                        var userdetails = Helpers.CreateToken.ReadToken(HttpContext);

                        if (requestbody.UpdateOperationDetails.Count > 0)
                        {
                            foreach (UpdateOperationDetails request in requestbody.UpdateOperationDetails)
                            {
                                if (!string.IsNullOrWhiteSpace(request.Position_Id))
                                {
                                    Guid Position_Id = Guid.Empty;
                                    if (!Guid.TryParse(request.Position_Id, out Position_Id))
                                    {
                                        response.ResponseStatus.Status = "Failure";
                                        response.ResponseStatus.ErrorMessage = "InValid Position_Id " + Convert.ToString(request.Position_Id);
                                        return BadRequest(response.ResponseStatus);
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Failure";
                                    response.ResponseStatus.ErrorMessage = "Position_Id cannot be blank";
                                    return BadRequest(response.ResponseStatus);
                                }
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Blank Request";
                            return BadRequest(response.ResponseStatus);
                        }
                        var result = await _bookingRepository.UpdateOperationDetails(requestbody, _configuration, userdetails);
                        if (result != null)
                        {
                            response = result;
                            if (result.ResponseStatus.Status == "Failure")
                            {
                                //return NotFound(response.ResponseStatus);
                                return Unauthorized();
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Success";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "No Records Found.";
                            return NotFound(response.ResponseStatus);
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Position_Ids can not be blank.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            return Ok(response);

        }

        /// <summary>
        /// Update Purchase Details
        /// </summary>
        /// <param name="requestbody"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdatePurchaseDetails")]
        [ProducesResponseType(typeof(UpdatePurchaseDetails_RS), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePurchaseDetails([FromBody] UpdatePurchaseDetails_RQ requestbody)
        {
            var response = new UpdatePurchaseDetails_RS();
            //bool isRequestOK = true;
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (requestbody != null)
                    {
                        var userdetails = Helpers.CreateToken.ReadToken(HttpContext);
                        if (requestbody.UpdatePurchaseDetails.Count > 0)
                        {
                            foreach (UpdatePurchaseDetails request in requestbody.UpdatePurchaseDetails)
                            {
                                if (!string.IsNullOrWhiteSpace(request.Position_Id))
                                {
                                    Guid Position_Id = Guid.Empty;
                                    Guid PBR_Id = Guid.Empty;
                                    Guid PP_Id = Guid.Empty;
                                    if (!Guid.TryParse(request.Position_Id, out Position_Id) && !Guid.TryParse(request.PBR_Id, out PBR_Id) && !Guid.TryParse(request.PP_Id, out PP_Id))
                                    {
                                        response.ResponseStatus.Status = "Failure";
                                        if (!Guid.TryParse(request.Position_Id, out Position_Id))
                                            response.ResponseStatus.ErrorMessage = "InValid Position_Id " + Convert.ToString(request.Position_Id) + ". ";
                                        if (!Guid.TryParse(request.PBR_Id, out PBR_Id))
                                            response.ResponseStatus.ErrorMessage += "InValid PBR_Id " + Convert.ToString(request.PBR_Id);
                                        if (!Guid.TryParse(request.PP_Id, out PP_Id))
                                            response.ResponseStatus.ErrorMessage += "InValid PP_Id " + Convert.ToString(request.PP_Id);
                                        return BadRequest(response.ResponseStatus);
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Failure";
                                    response.ResponseStatus.ErrorMessage = "Position_Id cannot be blank";
                                    return BadRequest(response.ResponseStatus);
                                }
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Blank Request";
                            return BadRequest(response.ResponseStatus);
                        }
                        var result = await _bookingRepository.UpdatePurchaseDetails(requestbody, _configuration, userdetails);
                        if (result != null)
                        {
                            response = result;
                            if (result.ResponseStatus.Status == "Failure")
                            {
                                //return NotFound(response.ResponseStatus);
                                return Unauthorized();
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Success";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "No Records Found.";
                            return NotFound(response.ResponseStatus);
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Position_Ids can not be blank.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            return Ok(response);

        }


        /// <summary>
        /// Update Position Product
        /// </summary>
        /// <param name="requestbody"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdatePositionProduct")]
        [ProducesResponseType(typeof(UpdatePositionProduct_RS), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePositionProduct([FromBody] UpdatePositionProduct_RQ requestbody)
        {
            var response = new UpdatePositionProduct_RS();
            //bool isRequestOK = true;
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (requestbody != null)
                    {
                        var userdetails = Helpers.CreateToken.ReadToken(HttpContext);
                        if (requestbody.UpdatePositionProduct.Count > 0)
                        {
                            foreach (UpdatePositionProduct request in requestbody.UpdatePositionProduct)
                            {
                                if (!string.IsNullOrWhiteSpace(request.Position_Id) && !string.IsNullOrWhiteSpace(request.Position_Id))
                                {
                                    Guid Position_Id = Guid.Empty;
                                    Guid Product_Id = Guid.Empty;
                                    if (!Guid.TryParse(request.Position_Id, out Position_Id) && !Guid.TryParse(request.Product_Id, out Product_Id))
                                    {
                                        response.ResponseStatus.Status = "Failure";
                                        if (!Guid.TryParse(request.Position_Id, out Position_Id))
                                            response.ResponseStatus.ErrorMessage = "InValid Position_Id " + Convert.ToString(request.Position_Id) + ". ";
                                        if (!Guid.TryParse(request.Product_Id, out Product_Id))
                                            response.ResponseStatus.ErrorMessage += "InValid Product_Id " + Convert.ToString(request.Product_Id);
                                        return BadRequest(response.ResponseStatus);
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Failure";
                                    if (!string.IsNullOrWhiteSpace(request.Position_Id))
                                        response.ResponseStatus.ErrorMessage = "Position_Id cannot be blank. ";
                                    if (!string.IsNullOrWhiteSpace(request.Product_Id))
                                        response.ResponseStatus.ErrorMessage += "Product_Id cannot be blank";
                                    return BadRequest(response.ResponseStatus);
                                }
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Blank Request";
                            return BadRequest(response.ResponseStatus);
                        }
                        var result = await _bookingRepository.UpdatePositionProduct(requestbody, _configuration, userdetails);
                        if (result != null)
                        {
                            response = result;
                            if (result.ResponseStatus.Status == "Failure")
                            {
                                //return NotFound(response.ResponseStatus);
                                return Unauthorized();
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Success";
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "No Records Found.";
                            return NotFound(response.ResponseStatus);
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Position_Ids can not be blank.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            return Ok(response);

        }
    }
}
