using AutoMapper;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.ViewModels;

namespace Web.mapper
{
    public class AccountProfile: Profile
    {
        public AccountProfile()
        {

            //Source=>Target
            CreateMap<RegisterViewModel, Owner>();
            CreateMap<SocialLoginViewModel, Owner>().ReverseMap();
        }
    }
}
