using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public class EventRepository : IEventRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        #endregion

        public EventRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
        }

        #region GetEventsByCityandDate
        public IQueryable<dynamic> GetEventsByCityandDate(EventRequest eventRequest)
        {
            var result = (from c in _MongoContext.mResort.AsQueryable()
                          join e in _MongoContext.mHotDate.AsQueryable() on c.Voyager_Resort_Id equals e.Resort_Id
                          where c.ResortType == "City" && c.Lookup.Trim().Contains(eventRequest.City.Trim()) &&
                          //   (e.StartDate >= eventRequest.Date && e.EndDate <= eventRequest.Date) &&
                            e.EndDate <= eventRequest.Date //&& e.StartDate >= eventRequest.Date                    
                          select new EventResponseProperties { City = c.Lookup, StartDate = e.StartDate, EndDate = e.EndDate, Name = e.Name, Type = e.Type });
            return result;
        }
        #endregion
    }
}
