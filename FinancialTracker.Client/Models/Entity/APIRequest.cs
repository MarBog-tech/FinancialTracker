using FinancialTracker.Client.Models.Utility;

namespace FinancialTracker.Client.Models.Entity;

public class APIRequest
{
    public SD.ApiType ApiType { get; set; } = SD.ApiType.GET;
    public string Url { get; set; }
    public object? Data { get; set; }
    public string Token { get; set; }
    public SD.ContentType ContentType { get; set; } = SD.ContentType.Json;
}