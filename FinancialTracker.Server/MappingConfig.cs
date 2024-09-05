using AutoMapper;
using FinancialTracker.Server.Models.Dto;
using FinancialTracker.Server.Models.Entity;

namespace FinancialTracker.Server;

public class MappingConfig : Profile
{
    public MappingConfig()
    {

        CreateMap<User, UserDTO>().ReverseMap();
        CreateMap<UserProfileDTO, UserProfile>().ReverseMap();
    }
}