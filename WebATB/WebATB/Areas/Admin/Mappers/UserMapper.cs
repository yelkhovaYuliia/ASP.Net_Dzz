using AutoMapper;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using WebATB.Areas.Admin.Models;
using WebATB.Areas.Admin.Models.Users;
using WebATB.Data.Entities.Idenity;
using WebATB.Data.Entities.Identity;

namespace WebATB.Areas.Admin.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<UserEntity, UserItemVM>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(x => x.Image, opt => opt.MapFrom(x => string.IsNullOrEmpty(x.Image) ? $"/images/noimage.png" : $"/avatars/200_{x.Image}"))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles!.Select(ur => ur.Role.Name).ToList()))
                // NEW: works with ProjectTo because EF can translate the Any(...) on the navigation
                .ForMember(dest => dest.IsGoogleLogin, opt => opt.MapFrom(src =>
                    src.Logins.Any(l => l.LoginProvider == GoogleDefaults.AuthenticationScheme)));

            CreateMap<UserLoginInfo, UserItemVM>()
                .ForMember(dest => dest.IsGoogleLogin, opt => opt.MapFrom(src => string.Equals(src.LoginProvider, GoogleDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)));
        }
    }
}