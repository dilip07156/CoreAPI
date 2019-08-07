using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IEventRepository
    {
        IQueryable<dynamic> GetEventsByCityandDate(EventRequest eventRequest);
    }
}
