using AutoMapper;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public class CommunityService : ICommunityService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CommunityService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfigurationService _configurationService;
        private readonly ITimeProvider _timeProvider;
        private readonly IWorkshopReviewReposistory _workshopReviewRepository;
        private readonly IWorkshopRepository _workshopRepository;

        public CommunityService(IUserRepository userRepository, ILogger<CommunityService> logger, IMapper mapper, IConfigurationService configurationService, ITimeProvider timeProvider, IWorkshopReviewReposistory workshopReviewRepository, IWorkshopRepository workshopRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
            _mapper = mapper;
            _configurationService = configurationService;
            _timeProvider = timeProvider;
            _workshopReviewRepository = workshopReviewRepository;
            _workshopRepository = workshopRepository;
        }

        public async Task<bool> ToggleLikeWorkshopAsync(int userId, int workshopId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            if (user.Workshops == null)
            {
                user.Workshops = new List<Workshop>();
            }

            if (user.Workshops.Any(w => w.Id == workshopId))
            {
                user.Workshops.Remove(user.Workshops.First(w => w.Id == workshopId));
            }
            else
            {
                user.Workshops.Add(new Workshop { Id = workshopId });
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return true;
        }

        public async Task<bool> PostWorkshopReviewAsync(int userId, int workshopId, string title, string desc, int rating, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return false;
            }

            var workshop = await _workshopRepository.GetByIdAsync(workshopId, null, cancellationToken);


            if (workshop == null || !workshop.WorkshopSchedules.Any(ws => ws.WorkshopTickets.Any(wt => wt.WorkshopParticipants.Any(wp => wp.ParticipantId == userId && wp.Status == "checked in"))))
            {
                return false;
            }

            var workshopReview = new WorkshopReview
            {
                Title = title,
                Description = desc,
                Rating = rating,
                CreatedOn = _timeProvider.Now,
                User = user,
                WorkshopId = workshopId
            };

            await _workshopReviewRepository.AddAsync(workshopReview, cancellationToken);

            return true;
        }

        public async Task<bool> PostWorkshopReviewResponseAsync(int hostId, int workshopReviewId, string response, CancellationToken cancellationToken = default)
        {
            var host = await _userRepository.GetByIdAsync(hostId, cancellationToken);
            if (host == null)
            {
                return false;
            }

            var workshopReview = await _workshopReviewRepository.GetByIdAsync(workshopReviewId, cancellationToken);
            if (workshopReview == null)
            {
                return false;
            }

            if (host.Workshops != null && !host.Workshops.Any(w => w.Id == workshopReview.WorkshopId))
            {
                return false;
            }

            workshopReview.Response = response;

            await _workshopReviewRepository.UpdateAsync(workshopReview, cancellationToken);

            return true;
        }

        public async Task<PagedResultDto<WorkshopReviewDto>> GetWorkshopReviewsAsync(int workshopId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var reviews = await _workshopReviewRepository.GetByWorkshopIdAsync(workshopId, page, pageSize, cancellationToken);
            return _mapper.MapPagedResult<WorkshopReview, WorkshopReviewDto>(reviews);
        }
    }
}
