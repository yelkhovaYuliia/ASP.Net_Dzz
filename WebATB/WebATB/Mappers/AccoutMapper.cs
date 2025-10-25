using AutoMapper;
using WebATB.Data.Entities.Idenity;
using WebATB.Data.Entities.Identity;
using WebATB.Models.Account;
namespace WebATB.Mappers;

public class AccoutMapper : Profile
{
    public AccoutMapper()
    {
        CreateMap<RegisterViewModel, UserEntity>()
            .ForMember(x=>x.UserName, opt=> opt.MapFrom(x=>x.Email))
            .ForMember(x=>x.Image, opt => opt.Ignore());

        CreateMap<UserEntity, UserLinkViewModel>()
            .ForMember(x => x.Name, opt => 
                opt.MapFrom(x=> $"{x.LastName} {x.FirstName}"))
            .ForMember(x=> x.Image, opt =>
                opt.MapFrom(x => x.Image ?? $"noimage.png"));
    }
}
