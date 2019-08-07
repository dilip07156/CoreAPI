using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Proxy;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Providers
{
    public class BookingProviders
    {
        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        #region Initializers 
        public BookingProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }
        #endregion

        #region Bridge Services
        /// <summary>
        /// Cancel Booking in SQL Bookings table
        /// </summary>
        /// <param name="objBookingsSetReq">Takes Booking Number, Username</param>
        /// <returns></returns>
        public async Task<ResponseStatus> CancelBookingDetails(BookingSetReq objBookingsSetReq)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Bookings:CancelBookingDetails"), objBookingsSetReq, typeof(ResponseStatus), "Bridge");
            return objResponseStatus;
        }

        /// <summary>
        /// Insert/Update Booking->Positions->Alternate Services in SQL PositionRequets table
        /// </summary>
        /// <param name="objBookingPosAltSetReq">Takes Booking Number,PositionId and Username</param>
        /// <returns></returns>
        public async Task<ResponseStatus> SetBookingAlternateServices(BookingPosAltSetReq objBookingPosAltSetReq)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Bookings:SetBookingAlternateServices"), objBookingPosAltSetReq, typeof(ResponseStatus), "Bridge");
            return objResponseStatus;
        }

        /// <summary>
        /// the below bridge service will 
        /// Update fields in PositionRequests table in sql
        /// UPSERT in documents and communicationlogs table in sql
        /// </summary>
        /// <param name="objBookingPosAltSetReq">Takes Booking Number,PositionId and Username</param>
        /// <returns></returns>
        public async Task<ResponseStatus> SetBookingAlternateServiceEmailDetails(BookingPosAltSetReq objBookingPosAltSetReq)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Bookings:SetBookingAlternateServiceEmailDetails"), objBookingPosAltSetReq, typeof(ResponseStatus), "Bridge");
            return objResponseStatus;
        }

        /// <summary>
        /// the below bridge service will 
        /// UPSERT details in Positions table in sql 
        /// </summary>
        /// <param name="objBookingPosAltSetReq">Takes Booking Number,PositionId and Username</param>
        /// <returns></returns>
        public async Task<ResponseStatus> SetBookingPositionDetails(BookingPosAltSetReq objBookingPosAltSetReq)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Bookings:SetBookingPositionDetails"), objBookingPosAltSetReq, typeof(ResponseStatus), "Bridge");
            return objResponseStatus;
        }

        /// <summary>
        /// the below bridge service will Update booking details in Bookings table in sql 
        /// </summary>
        /// <param name="objBookingSetReq">Takes Booking Number</param>
        /// <returns></returns>
        public async Task<ResponseStatus> SetBookingDetails(BookingSetReq objBookingSetReq)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Bookings:SetBookingDetails"), objBookingSetReq, typeof(ResponseStatus), "Bridge");
            return objResponseStatus;
        }
        #endregion
    }
}
