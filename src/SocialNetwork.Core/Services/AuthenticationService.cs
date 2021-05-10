using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Exceptions;
using SocialNetwork.Core.Repositories;

namespace SocialNetwork.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISignInManager _signInManager;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticationService(
            IUserRepository userRepository, 
            ISignInManager signInManager, 
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        public async Task RegisterAsync(User user, string password)
        {
            var existingUser = await _userRepository.GetUserAsync(user.Username);
            if (existingUser != null) throw new AuthenticationException($"User {user.Username} already exists");

            await CreateUserAsync(user, password);

            await _unitOfWork.CommitAsync();
        }

        public async Task CreateUserAsync(User user, string password)
        {
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
            user.RegisteredAt = DateTime.UtcNow;

            await _userRepository.AddUserAsync(user);
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetUserAsync(username);
            if (user == null) throw new AuthenticationException($"User {username} not found");

            var verificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, password);
            if (verificationResult != PasswordVerificationResult.Success) throw new AuthenticationException("Invalid password");

            await _signInManager.SignInAsync(user);

            return user;
        }
    }
}