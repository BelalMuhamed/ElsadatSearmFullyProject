using Application.DTOs.Notification;
using Application.DTOs.Profile;
using AutoMapper;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Mappings;
public class MappingProfile:Profile
{
    public MappingProfile()
    {
        //Profile

        CreateMap<ApplicationUser,ProfileDto>();
        CreateMap<UpdateProfileRequest,ApplicationUser>();

        //Notification
        CreateMap<Notification,NotificationDto>();

    }
}
