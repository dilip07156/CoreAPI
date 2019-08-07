using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IGenericRepository
    {
        QRFCounterResponse GetNextReferenceNumber(QRFCounterRequest request);

        //Task<MailGetRes> SendEmailAsync(EmailConfig email);

        AttributeValues getExchangeRate(string FromCurrencyId, string ToCurrencyId, string QRFID);

        bool getExchangeRateForPosition(ref List<mQRFPositionTotalCost> positionList);

        AttributeValues getExchangeRateFromBooking(string FromCurrencyId, string ToCurrencyId, string BookingNumber);

        GeocoderLocationGetRes GetLocationLatLon(string address);

        GeocoderLocationGetRes GetLocationLatLonByType(string address, string types);

        Task<DistanceMatrixGetRes> GetDistanceMatrix(DistanceMatrixGetReq req);

        Task<DistanceMatrixGetRes> GetDistanceMatrixForCity(string FromCityId, string ToCityId, string Units = "metric", string TransitMode = "bus");

        Task<DistanceMatrixGetRes> GetDistanceMatrixForProduct(string FromProductId, string ToProductId, string Units = "metric", string TransitMode = "bus");

        bool IsSalesOfficeUser(string UserName);

        Task<bool> DeletePositionPriceFOC(List<string> PositionIds, string UserName, bool IsClone = false, bool IsHardDelete = false);

        Task<PartnerCountryCityRes> GetPartnerCountryDetails(Attributes CountryInfo);

        Task<PartnerCountryCityRes> GetPartnerCityDetails(Attributes CityInfo);

        Task<PartnerCountryCityRes> GetPartnerCountryDetailsBasedOnCode(string Countrycode);

        Task<PartnerCountryCityRes> GetPartnerCityDetailsBasedOnCode(string CityCode);

        Task<PartnerCountryCityRes> GetPartnerCityDetailsBasedOnName(TravelogiCountryCityRes request);

        DateTime? ConvertStringToDateTime(string Date);
    }
}
