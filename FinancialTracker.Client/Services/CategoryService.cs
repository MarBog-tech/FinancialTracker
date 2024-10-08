using FinancialTracker.Client.Models.Entity;
using FinancialTracker.Client.Models.Utility;
using FinancialTracker.Client.Services.IServices;

namespace FinancialTracker.Client.Services;

public class CategoryService: ICategoryService
{
    private readonly IBaseService _baseService;
    private string? _url;

    public CategoryService(IConfiguration configuration, IBaseService baseService)
    {
        _baseService = baseService;
        _url = configuration["backend_url"] ?? configuration.GetValue<string>("ServiceUrls:API");
    }

    public async Task<T> GetAllAsync<T>()
    {
        return await _baseService.SendAsync<T>(new APIRequest()
        {
            ApiType = SD.ApiType.GET,
            Url = _url + "/Api/CategoryAPI"
        });
    }

    public async Task<T> GetAsync<T>(Guid id)
    {
        return await _baseService.SendAsync<T>(new APIRequest()
        {
            ApiType = SD.ApiType.GET,
            Url = _url + "/Api/CategoryAPI/" + id
        });
        ;
    }

    public async Task<T> CreateAsync<T>(Category dto)
    {
        return await _baseService.SendAsync<T>(new APIRequest()
        {
            ApiType = SD.ApiType.POST,
            Data = dto,
            Url = _url + "/Api/CategoryAPI"
        });
    }

    public async Task<T> UpdateAsync<T>(Category dto)
    {
        return await _baseService.SendAsync<T>(new APIRequest()
        {
            ApiType = SD.ApiType.PUT,
            Data = dto,
            Url = _url + "/Api/CategoryAPI/" + dto.Id
        });
    }

    public async Task<T> DeleteAsync<T>(Guid id)
    {
        return await _baseService.SendAsync<T>(new APIRequest()
        {
            ApiType = SD.ApiType.DELETE,
            Url = _url + "/Api/CategoryAPI/" + id
        });

    }
}