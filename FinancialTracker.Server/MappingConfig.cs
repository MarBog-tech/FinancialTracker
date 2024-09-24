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
        CreateMap<Transaction, TransactionIndexDTO>()
            .ForMember(dest=> dest.Category, 
                opt=> opt.MapFrom(
                    src=>src.Category.Name));
        CreateMap<TransactionDTO, Transaction>().ReverseMap();
    }
}