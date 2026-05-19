using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Utils
{
    public class DtoMapper : AutoMapper.Profile
    {
        public DtoMapper()
        {
            CreateMap<User, UserDto>();

            CreateMap<Workshop, WorkshopDisplayDto>()
                .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.Level, opt => opt.MapFrom(s => s.Level != null ? s.Level.Name : string.Empty))
                .ForMember(d => d.NextSchedule, opt => opt.MapFrom(s =>
                    s.WorkshopSchedules != null && s.WorkshopSchedules.Any()
                        ? s.WorkshopSchedules.OrderBy(ws => ws.StartOn).First().StartOn : default))
                .ForMember(d => d.PriceLower, opt => opt.MapFrom(s =>
                    (s.WorkshopSchedules != null &&
                     s.WorkshopSchedules.SelectMany(ws => ws.WorkshopTickets ?? Enumerable.Empty<WorkshopTicket>()).Any())
                        ? s.WorkshopSchedules.SelectMany(ws => ws.WorkshopTickets!).Min(t => t.Price)
                        : 0m))
                .ForMember(d => d.PriceUpper, opt => opt.MapFrom(s =>
                    (s.WorkshopSchedules != null &&
                     s.WorkshopSchedules.SelectMany(ws => ws.WorkshopTickets ?? Enumerable.Empty<WorkshopTicket>()).Any())
                        ? s.WorkshopSchedules.SelectMany(ws => ws.WorkshopTickets!).Max(t => t.Price)
                        : 0m))
                .ForMember(d => d.Rating, opt => opt.MapFrom(s =>
                    (s.WorkshopReviews != null && s.WorkshopReviews.Any())
                        ? s.WorkshopReviews.Average(r => (double)r.Rating)
                        : 0.0))
                .ForMember(d => d.ReviewCount, opt => opt.MapFrom(s => s.WorkshopReviews != null ? s.WorkshopReviews.Count : 0))
                .ForMember(d => d.Liked, opt => opt.MapFrom(s => s.Users != null && s.Users.Any()));
        }
    }
}
