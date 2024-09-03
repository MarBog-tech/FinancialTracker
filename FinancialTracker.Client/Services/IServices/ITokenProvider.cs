using FinancialTracker.Client.Models.Dto;

namespace FinancialTracker.Client.Services.IServices;

public interface ITokenProvider
{
    void SetToken(TokenDTO tokenDTO);
    TokenDTO? GetToken();
    void ClearToken();
}