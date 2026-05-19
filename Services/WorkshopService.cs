using AutoMapper;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Models.Responses;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public class WorkshopService : IWorkshopService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWorkshopRepository _workshopRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<WorkshopService> _logger;
        private readonly IMapper _mapper;

        public WorkshopService(IUserRepository userRepository, IWorkshopRepository workshopRepository, ITimeProvider timeProvider, IConfigurationService configurationService, ILogger<WorkshopService> logger, IMapper mapper)
        {
            _userRepository = userRepository;
            _workshopRepository = workshopRepository;
            _timeProvider = timeProvider;
            _configurationService = configurationService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PagedResult<WorkshopDisplayDto>> GetWorkshopAsync(
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
            return _mapper.MapPagedResult<Workshop, WorkshopDisplayDto>(await _workshopRepository.SearchAsync(query,
                locations, categories, levels, priceMin,
                priceMax, durationMin, durationMax, scheduleWithinDays,
                sortBy, sortDesc, userId, page, pageSize, cancellationToken));
        }
    }
}
