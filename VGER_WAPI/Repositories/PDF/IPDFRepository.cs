﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
   public interface IPDFRepository
    {
        Task<PDFGetRes> GeneratePDF(PDFGetReq request);
    }
}
