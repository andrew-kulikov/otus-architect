using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;

namespace SocialNetwork.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISignInManager _signInManager;

        public AuthenticationService(IUserRepository userRepository, ISignInManager signInManager)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
        }

        public async Task RegisterAsync(User user, string password)
        {
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
            user.RegisteredAt = DateTime.UtcNow;

            await _userRepository.AddUserAsync(user);
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetUserAsync(username);

            var verificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, password);
            if (verificationResult != PasswordVerificationResult.Success) throw new Exception("Invalid password");

            await _signInManager.SignInAsync(user);

            return user;
        }
    }
}