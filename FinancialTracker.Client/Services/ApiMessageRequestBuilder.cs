using System.Text;
using FinancialTracker.Client.Models.Entity;
using FinancialTracker.Client.Models.Utility;
using FinancialTracker.Client.Services.IServices;
using Newtonsoft.Json;

namespace FinancialTracker.Client.Services;

public class ApiMessageRequestBuilder : IApiMessageRequestBuilder
{
    public HttpRequestMessage Build(APIRequest apiRequest)
    {
        var message = new HttpRequestMessage
        {
            RequestUri = new Uri(apiRequest.Url)
        };
            
        message.Headers.Add("Accept",apiRequest.ContentType == SD.ContentType.MultipartFormData ? "*/*" : "application/json");

        if (apiRequest.ContentType == SD.ContentType.MultipartFormData)
        {
            using var content = new MultipartFormDataContent();

            foreach (var prop in apiRequest.Data.GetType().GetProperties())
            {
                var value = prop.GetValue(apiRequest.Data);

                if (value is FormFile file && file != null)
                {
                    content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                }
                else
                {
                    content.Add(new StringContent(value?.ToString() ?? ""), prop.Name);
                }
            }

            message.Content = content;
        }
        else
        {
            message.Content = apiRequest.Data != null
                ? new StringContent(JsonConvert.SerializeObject(apiRequest.Data), Encoding.UTF8, "application/json")
                : null;
        }

        // Визначення методу HTTP
        message.Method = apiRequest.ApiType switch
        {
            SD.ApiType.POST => HttpMethod.Post,
            SD.ApiType.PUT => HttpMethod.Put,
            SD.ApiType.DELETE => HttpMethod.Delete,
            _ => HttpMethod.Get,
        };
        return message;
    }
}