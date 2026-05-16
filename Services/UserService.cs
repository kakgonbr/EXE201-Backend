using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly AutoMapper.IMapper _mapper;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger, AutoMapper.IMapper mapper)
        {
            _userRepository = userRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<UserDto> GetUser(int id)
        {
            return _mapper.Map<UserDto>(await _userRepository.GetById(id));
        }

        public async Task<PagedResult<UserDto>> GetUsers(int page, int pageSize)
        {
            var users = await _userRepository.GetAll(page, pageSize);
            return _mapper.MapPagedResult<User, UserDto>(users);
        }
    }
}