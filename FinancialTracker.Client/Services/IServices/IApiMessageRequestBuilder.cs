using FinancialTracker.Client.Models.Entity;

namespace FinancialTracker.Client.Services.IServices;

public interface IApiMessageRequestBuilder
{
    HttpRequestMessage Build(APIRequest apiRequest);
}