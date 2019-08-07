using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI.Models;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Event")]
    public class EventController : Controller
    {
        #region Private Variable Declaration
        private readonly IEventRepository _eventRepository;
        private readonly IConfiguration _configuration;
        #endregion

        public EventController(IConfiguration configuration, IEventRepository eventRepository)
        {
            _configuration = configuration;
            _eventRepository = eventRepository;
        }

        /// <summary>
        /// Get Events By City and Date
        /// </summary>
        /// <param name="eventRequest"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetEventsByCityandDate")]
        public EventResponse GetEventsByCityandDate([FromBody] EventRequest eventRequest)
        {
            var response = new EventResponse();
            try
            {
                if (eventRequest != null && !string.IsNullOrEmpty(eventRequest.City) && eventRequest.Date != null)
                {
                    IQueryable<EventResponseProperties> result = (IQueryable<EventResponseProperties>)_eventRepository.GetEventsByCityandDate(eventRequest);
                    if (result != null && result.Count() > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.EventResponseProperties = result.ToList();
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }                                        
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "City/Date can not be blank.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " +ex.Message;
            }
            return response;
        }
    }
}