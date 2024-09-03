namespace FinancialTracker.Client.Models.Dto;

public class UserProfileDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}