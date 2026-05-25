using AutoMapper;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Models.Requests;
using EXE201_Backend.Repositories;
using System.Linq;

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
        private readonly IImageService _imageService;

        public WorkshopService(IUserRepository userRepository, IWorkshopRepository workshopRepository, IWorkshopScheduleRepository workshopScheduleRepository, ITimeProvider timeProvider, IConfigurationService configurationService, ILogger<WorkshopService> logger, IMapper mapper, IImageService imageService)
        {
            _userRepository = userRepository;
            _workshopRepository = workshopRepository;
            _workshopScheduleRepository = workshopScheduleRepository;
            _timeProvider = timeProvider;
            _configurationService = configurationService;
            _logger = logger;
            _mapper = mapper;
            _imageService = imageService;
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
                Status = string.IsNullOrWhiteSpace(request.Status) ? "pending" : request.Status.Trim(),
                WorkshopImages = request.ImageLinks != null
                    ? request.ImageLinks
                        .Where(link => !string.IsNullOrWhiteSpace(link))
                        .Select(link => new WorkshopImage { ImgLink = link!.Trim() })
                        .ToList()
                    : new List<WorkshopImage>()
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
            ValidateWorkshopRequiredFields(workshop);

            await _workshopRepository.UpdateAsync(workshop, cancellationToken);

            if (request.ImageLinks != null && request.ImageLinks.Any())
            {
                _imageService.ConsumeImage(userId);
            }

            return workshop.Id;
        }

        public async Task<PagedResultDto<WorkshopDetailsDto>> GetAllWorkshopsAsync(string? status = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var workshops = await _workshopRepository.GetAllPagedAsync(
                status,
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

        private static void ValidateWorkshopRequiredFields(Workshop workshop)
        {
            if (string.IsNullOrWhiteSpace(workshop.Title))
                throw new ArgumentException("Tên workshop là bắt buộc.");

            if (workshop.CategoryId <= 0)
                throw new ArgumentException("Danh mục là bắt buộc.");

            var schedule = workshop.WorkshopSchedules?
                .OrderBy(s => s.StartOn)
                .FirstOrDefault();

            if (schedule == null)
                throw new ArgumentException("Lịch trình là bắt buộc.");

            var ticket = schedule.WorkshopTickets?.FirstOrDefault();

            if (ticket == null)
                throw new ArgumentException("Vé là bắt buộc.");

            if (ticket.EndTime <= ticket.StartTime)
                throw new ArgumentException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

            if (ticket.Price <= 0)
                throw new ArgumentException("Giá vé là bắt buộc và phải lớn hơn 0.");

            if (ticket.MaxTickets <= 0)
                throw new ArgumentException("Số lượng vé là bắt buộc và phải lớn hơn 0.");
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

            if (!string.IsNullOrWhiteSpace(request.Description))
                workshop.Description = request.Description.Trim();

            if (!string.IsNullOrWhiteSpace(request.Location))
                workshop.Location = request.Location.Trim();

            if (!string.IsNullOrWhiteSpace(request.ThumbnailLink))
                workshop.ThumbnailLink = request.ThumbnailLink.Trim();

            if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
                workshop.CategoryId = request.CategoryId.Value;

            if (request.LevelId.HasValue && request.LevelId.Value > 0)
                workshop.LevelId = request.LevelId.Value;

            if (!string.IsNullOrWhiteSpace(request.Language))
                workshop.Language = request.Language.Trim();

            if (!string.IsNullOrWhiteSpace(request.Status))
                workshop.Status = request.Status.Trim();

            if (request.ImageLinks != null && request.ImageLinks.Any())
            {
                workshop.WorkshopImages ??= new List<WorkshopImage>();
                var existingLinks = workshop.WorkshopImages
                    .Where(img => !string.IsNullOrWhiteSpace(img.ImgLink))
                    .Select(img => img.ImgLink.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var imageLink in request.ImageLinks.Where(link => !string.IsNullOrWhiteSpace(link)).Select(link => link.Trim()))
                {
                    if (!existingLinks.Contains(imageLink))
                    {
                        workshop.WorkshopImages.Add(new WorkshopImage { ImgLink = imageLink });
                        existingLinks.Add(imageLink);
                    }
                }
            }

            var scheduleReq = request.Schedules?.FirstOrDefault();

            if (scheduleReq != null)
            {
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

                if (!string.IsNullOrWhiteSpace(scheduleReq.StartOn))
                {
                    if (!DateOnly.TryParse(scheduleReq.StartOn, out var startOn))
                    {
                        throw new ArgumentException($"Invalid StartOn date: {scheduleReq.StartOn}");
                    }
                    schedule.StartOn = startOn;
                }

                var ticketReq = scheduleReq.Tickets?.FirstOrDefault();

                if (ticketReq != null)
                {
                    schedule.WorkshopTickets ??= new List<WorkshopTicket>();

                    var ticket = schedule.WorkshopTickets.FirstOrDefault();

                    if (ticket == null)
                    {
                        ticket = new WorkshopTicket();
                        schedule.WorkshopTickets.Add(ticket);
                    }

                    if (!string.IsNullOrWhiteSpace(ticketReq.TicketType))
                    {
                        ticket.TicketType = ticketReq.TicketType.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(ticketReq.StartTime))
                    {
                        if (!TimeOnly.TryParse(ticketReq.StartTime, out var startTime))
                            throw new ArgumentException($"Invalid ticket StartTime: {ticketReq.StartTime}");
                        ticket.StartTime = startTime;
                    }

                    if (!string.IsNullOrWhiteSpace(ticketReq.EndTime))
                    {
                        if (!TimeOnly.TryParse(ticketReq.EndTime, out var endTime))
                            throw new ArgumentException($"Invalid ticket EndTime: {ticketReq.EndTime}");
                        ticket.EndTime = endTime;
                    }

                    if (ticketReq.MaxTickets.HasValue && ticketReq.MaxTickets.Value > 0)
                    {
                        ticket.MaxTickets = ticketReq.MaxTickets.Value;
                    }

                    if (ticketReq.Price.HasValue && ticketReq.Price.Value >= 0)
                    {
                        ticket.Price = ticketReq.Price.Value;
                    }

                    var duration = ticket.EndTime.ToTimeSpan() - ticket.StartTime.ToTimeSpan();
                    workshop.Duration = (int)Math.Max(0, duration.TotalMinutes);
                }
            }

            await _workshopRepository.UpdateAsync(workshop, cancellationToken);

            if (request.ImageLinks != null && request.ImageLinks.Any())
            {
                _imageService.ConsumeImage(userId);
            }

            return true;
        }

        public async Task<bool> DeleteWorkshopAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            var workshop = await _workshopRepository.GetByIdAsync(id, userId, cancellationToken);
            if (workshop == null)
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return false;
            }

            if (workshop.CreatedBy != userId && user.Role != "staff")
            {
                throw new UnauthorizedAccessException("You can only delete your own workshops.");
            }

            if (workshop.WorkshopImages != null)
            {
                foreach (var image in workshop.WorkshopImages.Where(img => !string.IsNullOrWhiteSpace(img.ImgLink)))
                {
                    _imageService.DeleteImageFile(image.ImgLink.Trim());
                }
            }

            if (!string.IsNullOrWhiteSpace(workshop.ThumbnailLink))
            {
                _imageService.DeleteImageFile(workshop.ThumbnailLink.Trim());
            }

            await _workshopRepository.DeleteAsync(id, cancellationToken);
            return true;
        }

        public async Task<bool> VerifyWorkshopAsync(int id, CancellationToken cancellationToken = default)
        {
            var workshop = await _workshopRepository.GetByIdAsync(id, null, cancellationToken);
            if (workshop == null)
            {
                return false;
            }
            workshop.Status = "verified";
            await _workshopRepository.UpdateAsync(workshop, cancellationToken);
            return true;
        }
    }
}