using AutoMapper;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Models.Requests;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public class WorkshopService : IWorkshopService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWorkshopRepository _workshopRepository;
        private readonly IWorkshopScheduleRepository _workshopScheduleRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<WorkshopService> _logger;
        private readonly IMapper _mapper;

        public WorkshopService(IUserRepository userRepository, IWorkshopRepository workshopRepository, IWorkshopScheduleRepository workshopScheduleRepository, ITimeProvider timeProvider, IConfigurationService configurationService, ILogger<WorkshopService> logger, IMapper mapper)
        {
            _userRepository = userRepository;
            _workshopRepository = workshopRepository;
            _workshopScheduleRepository = workshopScheduleRepository;
            _timeProvider = timeProvider;
            _configurationService = configurationService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkshopDisplayDto>> GetRecommendedWorkshopsAsync(int? userId, CancellationToken cancellationToken = default)
        {
            var recommendedWorkshops = await _workshopRepository.GetRecommendationsAsync(userId, cancellationToken);
            return _mapper.Map<IEnumerable<WorkshopDisplayDto>>(recommendedWorkshops);
        }

        public async Task<WorkshopDetailsDto?> GetWorkshopByIdAsync(int id, int? userId = null, CancellationToken cancellationToken = default)
        {
            var workshop = await _workshopRepository.GetByIdAsync(id, userId, cancellationToken);
            if (workshop == null) return null;
            return _mapper.Map<WorkshopDetailsDto>(workshop);
        }

        public async Task<PagedResultDto<WorkshopDisplayDto>> GetAllWorkshopsAsync(string? status = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var pagedWorkshops = await _workshopRepository.GetAllWorkshopsAsync(status, page, pageSize, cancellationToken);
            return _mapper.MapPagedResult<Workshop, WorkshopDisplayDto>(pagedWorkshops);
        }

        public async Task<PagedResultDto<WorkshopDisplayDto>> GetWorkshopAsync(
            string? query = null,
            IEnumerable<string>? locations = null,
            IEnumerable<string>? categories = null,
            IEnumerable<string>? levels = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int? durationMin = null,
            int? durationMax = null,
            int? scheduleWithinDays = null,
            WorkshopSort? sortBy = null,
            bool sortDesc = false,
            int userId = 0,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return _mapper.MapPagedResult<Workshop, WorkshopDisplayDto>(await _workshopRepository.SearchAsync(
                query, locations, categories, levels, priceMin,
                priceMax, durationMin, durationMax, scheduleWithinDays,
                sortBy, sortDesc, userId, page, pageSize, cancellationToken));
        }

        public async Task<int> CreateWorkshopAsync(CreateWorkshopRequest request, int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var workshop = new Workshop
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                Location = request.Location.Trim(),

                ThumbnailLink = request.ThumbnailLink?.Trim(),
                CategoryId = request.CategoryId,
                LevelId = request.LevelId,
                Language = string.IsNullOrWhiteSpace(request.Language) ? "en" : request.Language.Trim(),
                CreatedBy = userId,
                Status = "draft"
            };

            await _workshopRepository.AddAsync(workshop, cancellationToken);

            int maxDurationMinutes = 0;

            if (request.Schedules != null)
            {
                foreach (var schReq in request.Schedules)
                {
                    if (!DateOnly.TryParse(schReq.StartOn, out var startOn))
                    {
                        throw new ArgumentException($"Invalid StartOn date: {schReq.StartOn}");
                    }

                    var schedule = new WorkshopSchedule
                    {
                        WorkshopId = workshop.Id,
                        StartOn = startOn,
                        CreatedFromRepeat = false
                    };

                    var tickets = new List<WorkshopTicket>();

                    if (schReq.Tickets != null)
                    {
                        foreach (var tReq in schReq.Tickets)
                        {
                            if (!TimeOnly.TryParse(tReq.StartTime, out var st))
                                throw new ArgumentException($"Invalid ticket StartTime: {tReq.StartTime}");
                            if (!TimeOnly.TryParse(tReq.EndTime, out var et))
                                throw new ArgumentException($"Invalid ticket EndTime: {tReq.EndTime}");

                            var ticket = new WorkshopTicket
                            {
                                TicketType = tReq.TicketType,
                                StartTime = st,
                                EndTime = et,
                                MaxTickets = tReq.MaxTickets,
                                Price = tReq.Price
                            };

                            tickets.Add(ticket);
                        }
                    }

                    schedule.WorkshopTickets = tickets;

                    await _workshopScheduleRepository.AddAsync(schedule, cancellationToken);

                    if (tickets.Any())
                    {
                        var minStart = tickets.Min(t => t.StartTime);
                        var maxEnd = tickets.Max(t => t.EndTime);
                        var span = maxEnd.ToTimeSpan() - minStart.ToTimeSpan();
                        var minutes = (int)Math.Max(0, span.TotalMinutes);
                        if (minutes > maxDurationMinutes) maxDurationMinutes = minutes;
                    }
                }
            }

            workshop.Duration = maxDurationMinutes;
            await _workshopRepository.UpdateAsync(workshop, cancellationToken);

            return workshop.Id;
        }

        public async Task<PagedResultDto<WorkshopDetailsDto>> GetAllWorkshopsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var workshops = await _workshopRepository.GetAllPagedAsync(
                page,
                pageSize,
                cancellationToken
            );

            return _mapper.MapPagedResult<Workshop, WorkshopDetailsDto>(workshops);
        }

        public async Task<PagedResultDto<WorkshopDetailsDto>> GetWorkshopsByUserIdAsync(int userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var workshops = await _workshopRepository.GetByUserIdPagedAsync(
                userId,
                page,
                pageSize,
                cancellationToken
            );

            return _mapper.MapPagedResult<Workshop, WorkshopDetailsDto>(workshops);
        }

        public async Task<bool> UpdateWorkshopAsync(
    int id,
    UpdateWorkshopRequest request,
    int userId,
    CancellationToken cancellationToken = default)
        {
            var workshop = await _workshopRepository.GetByIdAsync(id, userId, cancellationToken);
            if (workshop == null)
            {
                return false;
            }

            if (workshop.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own workshops.");
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
                workshop.Title = request.Title.Trim();

            workshop.Description = request.Description?.Trim();

            if (!string.IsNullOrWhiteSpace(request.Location))
                workshop.Location = request.Location.Trim();

            workshop.ThumbnailLink = string.IsNullOrWhiteSpace(request.ThumbnailLink)
                ? null
                : request.ThumbnailLink.Trim();

            if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
                workshop.CategoryId = request.CategoryId.Value;

            if (request.LevelId.HasValue && request.LevelId.Value > 0)
                workshop.LevelId = request.LevelId.Value;

            if (!string.IsNullOrWhiteSpace(request.Language))
                workshop.Language = request.Language.Trim();

            if (!string.IsNullOrWhiteSpace(request.Status))
                workshop.Status = request.Status.Trim();

            var scheduleReq = request.Schedules?.FirstOrDefault();

            if (scheduleReq != null)
            {
                if (!DateOnly.TryParse(scheduleReq.StartOn, out var startOn))
                {
                    throw new ArgumentException($"Invalid StartOn date: {scheduleReq.StartOn}");
                }

                workshop.WorkshopSchedules ??= new List<WorkshopSchedule>();

                var schedule = workshop.WorkshopSchedules
                    .OrderBy(s => s.StartOn)
                    .FirstOrDefault();

                if (schedule == null)
                {
                    schedule = new WorkshopSchedule
                    {
                        WorkshopId = workshop.Id,
                        CreatedFromRepeat = false
                    };

                    workshop.WorkshopSchedules.Add(schedule);
                }

                schedule.StartOn = startOn;

                var ticketReq = scheduleReq.Tickets?.FirstOrDefault();

                if (ticketReq != null)
                {
                    if (!TimeOnly.TryParse(ticketReq.StartTime, out var startTime))
                        throw new ArgumentException($"Invalid ticket StartTime: {ticketReq.StartTime}");

                    if (!TimeOnly.TryParse(ticketReq.EndTime, out var endTime))
                        throw new ArgumentException($"Invalid ticket EndTime: {ticketReq.EndTime}");

                    schedule.WorkshopTickets ??= new List<WorkshopTicket>();

                    var ticket = schedule.WorkshopTickets.FirstOrDefault();

                    if (ticket == null)
                    {
                        ticket = new WorkshopTicket();
                        schedule.WorkshopTickets.Add(ticket);
                    }

                    ticket.TicketType = string.IsNullOrWhiteSpace(ticketReq.TicketType)
                        ? "standard"
                        : ticketReq.TicketType.Trim();

                    ticket.StartTime = startTime;
                    ticket.EndTime = endTime;
                    ticket.MaxTickets = ticketReq.MaxTickets;
                    ticket.Price = ticketReq.Price;

                    var duration = endTime.ToTimeSpan() - startTime.ToTimeSpan();
                    workshop.Duration = (int)Math.Max(0, duration.TotalMinutes);
                }
            }

            await _workshopRepository.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteWorkshopAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            var workshop = await _workshopRepository.GetByIdAsync(id, userId, cancellationToken);
            if (workshop == null)
            {
                return false;
            }

            if (workshop.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own workshops.");
            }

            await _workshopRepository.DeleteAsync(id, cancellationToken);
            return true;
        }
    }
}
