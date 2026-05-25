using AutoMapper;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public class HostService : IHostService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHostRegistrationRepository _hostRegistrationRepository;
        private readonly IMailService _mailService;
        private readonly ITimeProvider _timeProvider;
        private readonly IConfigurationService _configurationService;
        private readonly IMapper _mapper;

        public HostService(IUserRepository userService, ITimeProvider timeProvider, IConfigurationService configurationService, IMapper mapper, IHostRegistrationRepository hostRegistrationRepository, IMailService mailService)
        {
            _userRepository = userService;
            _timeProvider = timeProvider;
            _configurationService = configurationService;
            _mapper = mapper;
            _hostRegistrationRepository = hostRegistrationRepository;
            _mailService = mailService;
        }

        public async Task<bool> RegisterHostAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return false;
            }

            var hostRegistration = new HostRegistration
            {
                UserId = userId,
                CreatedOn = _timeProvider.Now,
                Approved = false
            };

            await _hostRegistrationRepository.AddAsync(hostRegistration, cancellationToken);

            await _mailService.SendHostRegistration(user.Email, cancellationToken);

            return true;
        }

        public async Task<bool> UpdateHostRegistrationAsync(int staffId, int userId, bool approved, string? note, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return false;
            }

            var hostRegistration = await _hostRegistrationRepository.GetByUserIdAsync(userId, cancellationToken);

            if (hostRegistration == null)
            {
                return false;
            }

            hostRegistration.Approved = approved;
            hostRegistration.Note = note;
            hostRegistration.ApprovedBy = staffId;
            hostRegistration.UpdatedOn = _timeProvider.Now;
            await _hostRegistrationRepository.UpdateAsync(hostRegistration, cancellationToken);

            if (approved)
            {
                user.Role = "host";

                await _userRepository.UpdateAsync(user, cancellationToken);

                await _mailService.SendHostAccepted(user.Email, cancellationToken);
            }
            else
            {
                await _mailService.SendHostRejected(user.Email, cancellationToken);
            }

            return true;
        }

        public async Task<PagedResultDto<HostRegistrationDto>> GetHostRegistrations(
            ApproveStatusFilter? approveFilter = null,
            HostRegistrationSort? sortBy = null,
            bool sortDesc = false, int page = 1,
            int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var pagedHostRegistrations = await _hostRegistrationRepository.GetAllAsync(approveFilter, sortBy, sortDesc, page, pageSize, cancellationToken);
            return _mapper.MapPagedResult<HostRegistration, HostRegistrationDto>(pagedHostRegistrations);
        }
    }
}
