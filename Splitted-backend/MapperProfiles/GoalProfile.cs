using AutoMapper;
using Models.DTOs.Incoming.Goal;
using Models.DTOs.Outgoing.Goal;
using Models.Entities;
using System.Diagnostics;

namespace Splitted_backend.MapperProfiles
{
    public class GoalProfile : Profile
    {
        public GoalProfile()
        {
            CreateMap<GoalPostDTO, Goal>()
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => DateTime.Today))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category == null ? null : src.Category.ToLower()));

            CreateMap<GoalPutDTO, Goal>()
                .ForMember(dest => dest.Amount, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Amount))))
                .ForMember(dest => dest.Deadline, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.Deadline))))
                .ForMember(dest => dest.IsMain, opt => opt.Condition(src => src.SetProperties.Contains(nameof(src.IsMain))));

            CreateMap<Goal, GoalCreatedDTO>();

            CreateMap<Goal, GoalGetDTO>();

        }
    }
}
