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
                        ? s.WorkshopSchedules.OrderBy(s => s.StartOn).First().StartOn : default))
                .ForMember(d => d.PriceLower, opt => opt.MapFrom(s =>
                    (s.WorkshopSchedules != null &&
                     s.WorkshopSchedules.SelectMany(s => s.WorkshopTickets ?? Enumerable.Empty<WorkshopTicket>()).Any())
                        ? s.WorkshopSchedules.SelectMany(s => s.WorkshopTickets!).Min(t => t.Price)
                        : 0m))
                .ForMember(d => d.PriceUpper, opt => opt.MapFrom(s =>
                    (s.WorkshopSchedules != null &&
                     s.WorkshopSchedules.SelectMany(s => s.WorkshopTickets ?? Enumerable.Empty<WorkshopTicket>()).Any())
                        ? s.WorkshopSchedules.SelectMany(s => s.WorkshopTickets!).Max(t => t.Price)
                        : 0m))
                .ForMember(d => d.Rating, opt => opt.MapFrom(s =>
                    (s.WorkshopReviews != null && s.WorkshopReviews.Any())
                        ? s.WorkshopReviews.Average(r => (double)r.Rating)
                        : 0.0))
                .ForMember(d => d.ReviewCount, opt => opt.MapFrom(s => s.WorkshopReviews != null ? s.WorkshopReviews.Count : 0))
                .ForMember(d => d.Liked, opt => opt.MapFrom(s => s.Users != null && s.Users.Any()));

            CreateMap<WorkshopSchedule, WorkshopScheduleDto>()
                .ForMember(d => d.RemainingTickets, opt => opt.MapFrom(s =>
                    (s.WorkshopTickets != null)
                        ? s.WorkshopTickets.Sum(wt => wt.MaxTickets) - s.WorkshopTickets.SelectMany(wt => wt.WorkshopParticipants).Where(wp => wp.Status == "paid").Count()
                        : 0))
                .ForMember(d => d.PriceLower, opt => opt.MapFrom(s =>
                    (s.WorkshopTickets != null && s.WorkshopTickets.Any())
                        ? s.WorkshopTickets.Min(t => t.Price)
                        : 0m))
                .ForMember(d => d.PriceUpper, opt => opt.MapFrom(s =>
                    (s.WorkshopTickets != null && s.WorkshopTickets.Any())
                        ? s.WorkshopTickets.Max(t => t.Price)
                        : 0m));

            CreateMap<Workshop, WorkshopDetailsDto>()
                .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.Level, opt => opt.MapFrom(s => s.Level != null ? s.Level.Name : string.Empty))
                .ForMember(d => d.Schedules, opt => opt.MapFrom(s =>
                    s.WorkshopSchedules == null || s.WorkshopSchedules.Count == 0
                        ? new List<WorkshopSchedule>()
                        : s.WorkshopSchedules))
                .ForMember(d => d.Rating, opt => opt.MapFrom(s =>
                    (s.WorkshopReviews != null && s.WorkshopReviews.Any())
                        ? s.WorkshopReviews.Average(r => (double)r.Rating)
                        : 0.0))
                .ForMember(d => d.ReviewCount, opt => opt.MapFrom(s => s.WorkshopReviews != null ? s.WorkshopReviews.Count : 0))
                .ForMember(d => d.Liked, opt => opt.MapFrom(s => s.Users != null && s.Users.Any()));

            CreateMap<WorkshopTicket, WorkshopTicketDto>()
    .ForMember(d => d.RemainingTickets, opt => opt.MapFrom(s =>
        s.MaxTickets -
        (s.WorkshopParticipants != null
            ? s.WorkshopParticipants.Count(wp => wp.Status == "paid")
            : 0)));

            CreateMap<WorkshopSchedule, WorkshopScheduleDetailsDto>()
                .ForMember(d => d.Tickets, opt => opt.MapFrom(s =>
                    s.WorkshopTickets == null || s.WorkshopTickets.Count == 0
                        ? new List<WorkshopTicket>()
                        : s.WorkshopTickets))
                .ForMember(d => d.WorkshopTitle, opt => opt.MapFrom(s => s.Workshop != null ? s.Workshop.Title : string.Empty))
                .ForMember(d => d.WorkshopThumbnailLink, opt => opt.MapFrom(s => s.Workshop != null ? s.Workshop.ThumbnailLink : string.Empty))
                .ForMember(d => d.WorkshopLocation, opt => opt.MapFrom(s => s.Workshop != null ? s.Workshop.Location : string.Empty));

            CreateMap<WorkshopReview, WorkshopReviewDto>()
                .ForMember(d => d.ReviewerName, opt => opt.MapFrom(s => s.User.Name))
                .ForMember(d => d.ReviewerId, opt => opt.MapFrom(s => s.UserId))
                .ForMember(d => d.ReviewerAvatarLink, opt => opt.MapFrom(s => s.User.AvatarLink));
        }
    }
}
