using AutoMapper;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IWorkshopParticipantRepository _workshopParticipantRepository;
        private readonly IWorkshopScheduleRepository _workshopScheduleRepository;
        private readonly IWorkshopTicketRepository _workshopTicketRepository;
        private readonly IConfigurationService _configurationService;
        private readonly ITimeProvider _timeProvider;
        private readonly IMapper _mapper;

        public ScheduleService(
            IWorkshopParticipantRepository workshopParticipantRepository,
            IWorkshopScheduleRepository workshopScheduleRepository,
            IWorkshopTicketRepository workshopTicketRepository,
            IConfigurationService configurationService,
            ITimeProvider timeProvider,
            IMapper mapper)
        {
            _workshopParticipantRepository = workshopParticipantRepository;
            _workshopScheduleRepository = workshopScheduleRepository;
            _workshopTicketRepository = workshopTicketRepository;
            _configurationService = configurationService;
            _timeProvider = timeProvider;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkshopScheduleDto>> GetSchedulesInMonthAsync(int userId, int month, CancellationToken cancellationToken = default)
        {
            var schedules = await _workshopScheduleRepository.GetSchedulesInMonthAsync(userId, month, cancellationToken);
            return _mapper.Map<IEnumerable<WorkshopScheduleDto>>(schedules);
        }

        public async Task<WorkshopScheduleDetailsDto> GetScheduleDetailsAsync(int scheduleId, CancellationToken cancellationToken = default)
        {
            var schedule = await _workshopScheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
            return _mapper.Map<WorkshopScheduleDetailsDto>(schedule);
        }

        public async Task<bool> IsUserOccupiedAsync(int userId, int ticketId, CancellationToken cancellationToken = default)
        {
            return await _workshopScheduleRepository.IsUserOccupiedAsync(userId, ticketId, cancellationToken);
        }

        public async Task<PagedResultDto<WorkshopScheduleDetailsDto>> GetUpcoming(int userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var pagedResult = await _workshopScheduleRepository.GetUpcoming(userId, page, pageSize, cancellationToken);
            return _mapper.MapPagedResult<WorkshopSchedule, WorkshopScheduleDetailsDto>(pagedResult);
        }
    }
}
