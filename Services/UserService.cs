using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;
using System.Text.RegularExpressions;

namespace EXE201_Backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IImageService _imageService;
        private readonly ILogger<UserService> _logger;
        private readonly AutoMapper.IMapper _mapper;

        public UserService(IUserRepository userRepository, IImageService imageService, ILogger<UserService> logger, AutoMapper.IMapper mapper)
        {
            _userRepository = userRepository;
            _imageService = imageService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<UserDto> GetUser(int id, CancellationToken cancellationToken = default)
        {
            return _mapper.Map<UserDto>(await _userRepository.GetByIdAsync(id, cancellationToken));
        }

        public async Task<PagedResultDto<UserDto>> GetUsers(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync(page, pageSize, cancellationToken);
            return _mapper.MapPagedResult<User, UserDto>(users);
        }

        public async Task<bool> ChangeUsername(int userId, string newName, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.Name = newName;
            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<bool> ChangePhonenumber(int userId, string newPhoneNumber, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            newPhoneNumber = NormalizePhone(newPhoneNumber);

            if (!IsValidPhone(newPhoneNumber))
            {
                return false;
            }

            user.PhoneNumber = newPhoneNumber;
            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<bool> ChangeAvatar(int userId, string newAvatarUrl, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.AvatarLink = newAvatarUrl;
            await _userRepository.UpdateAsync(user, cancellationToken);

            _imageService.ConsumeImage(userId);

            return true;
        }

        public async Task<bool> ChangeLocation(int userId, string newLocation, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.Location = newLocation;
            await _userRepository.UpdateAsync(user, cancellationToken);

            return true;
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            if (Regex.IsMatch(phone, @"^\+\d{1,3}\d{6,14}$"))
            {
                return true;
            }

            return false;
        }

        private static string NormalizePhone(string phone, string defaultCountryCode = "84")
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            phone = phone.Replace(" ", "");

            if (Regex.IsMatch(phone, @"^0\d{9}$"))
            {
                return $"+{defaultCountryCode}{phone.Substring(1)}";
            }

            Match match = Regex.Match(phone, @"^\+(\d{1,3})(\d{6,14})$");

            if (match.Success)
            {
                return $"+{match.Groups[1].Value}{match.Groups[2].Value}";
            }

            if (Regex.IsMatch(phone, @"^\+\d{1,3}\s\d{6,14}$"))
            {
                return phone;
            }

            return phone;
        }
    }
}