using AutoMapper;
using MongoDbApp.Models;
using MongoDbApp.ViewModels;

namespace MongoDbApp.Profiles;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, UserListViewModel>();
        CreateMap<User, UserDetailsViewModel>();
        CreateMap<UserCreateViewModel, User>();
        CreateMap<UserUpdateViewModel, User>();
    }
}
