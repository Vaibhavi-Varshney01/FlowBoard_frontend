using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlowBoard.Auth.Config;
using FlowBoard.Auth.Exceptions;
using FlowBoard.Auth.Models;
using FlowBoard.Auth.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using FlowBoard.Auth.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Auth.Services
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public AuthServiceImpl(
            IUserRepository userRepository,
            IOptions<JwtSettings> jwtSettings,
            IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
            _passwordHasher = passwordHasher;
        }

        //REGISTER
        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            if (await _userRepository.ExistsByEmail(request.Email))
                throw new EmailAlreadyExistsException();

            if (await _userRepository.ExistsByUsername(request.Username))
                throw new UsernameAlreadyExistsException();

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Username,
                Role = "MEMBER",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Provider = "LOCAL"
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.Save(user);

            return GenerateAuthResponse(user);
        }

        //LOGIN
        public async Task<AuthResponse> Login(string email, string password, string? requestedRole = null)
        {
            var user = await _userRepository.FindByEmail(email)
                ?? throw new InvalidCredentialsException();

            if (!user.IsActive)
                throw new AccountDeactivatedException();

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash!,
                password
            );

            if (result == PasswordVerificationResult.Failed)
                throw new InvalidCredentialsException();

            // GLOBAL ADMIN PROMOTION: If requestedRole is ADMIN, update user in DB
            if (!string.IsNullOrEmpty(requestedRole) && user.Role != requestedRole)
            {
                user.Role = requestedRole;
                await _userRepository.Update(user);
            }

            return GenerateAuthResponse(user);
        }

        //GET USER BY ID
        public async Task<ApplicationUser> GetUserById(string userId)
        {
            return await _userRepository.FindByUserId(userId)
                ?? throw new UserNotFoundException();
        }

        //GET USER BY EMAIL
        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await _userRepository.FindByEmail(email)
                ?? throw new UserNotFoundException();
        }

        // UPDATE PROFILE
        public async Task<ApplicationUser> UpdateProfile(string userId, UpdateProfileRequest request)
        {
            var user = await _userRepository.FindByUserId(userId)
                ?? throw new UserNotFoundException();

            user.FullName = request.FullName;
            user.UserName = request.Username;
            user.AvatarUrl = request.AvatarUrl;

            return await _userRepository.Update(user);
        }

        //CHANGE PASSWORD
        public async Task ChangePassword(ChangePasswordRequest request)
        {
            var user = await _userRepository.FindByUserId(request.UserId)
                ?? throw new UserNotFoundException();

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash!,
                request.OldPassword
            );

            if (result == PasswordVerificationResult.Failed)
                throw new InvalidCredentialsException("Old password is incorrect");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            await _userRepository.Update(user);
        }

        //DEACTIVATE ACCOUNT
        public async Task DeactivateAccount(string userId)
        {
            var user = await _userRepository.FindByUserId(userId)
                ?? throw new UserNotFoundException();

            user.IsActive = false;

            await _userRepository.Update(user);
        }

        //SEARCH USERS
        public async Task<List<ApplicationUser>> SearchUsers(string query)
        {
            return await _userRepository.SearchByFullName(query);
        }

        //VALIDATE TOKEN
        public Task<bool> ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("FlowBoardProjectSecretKey2026!!!!");

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        //REFRESH TOKEN
        public async Task<string> RefreshToken(string token)
        {
            var isValid = await ValidateToken(token);

            if (!isValid)
                throw new InvalidTokenException();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var email = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidTokenException();

            var user = await _userRepository.FindByEmail(email)
                ?? throw new UserNotFoundException();

            return GenerateJwt(user);
        }

        //OAUTH2 — Find or create user from external provider
        public async Task<AuthResponse> HandleOAuthLogin(string email, string fullName, string provider, string avatarUrl, string? requestedRole = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required for OAuth login.");

            var user = await _userRepository.FindByEmail(email);

            if (user == null)
            {
                var baseUsername = string.IsNullOrEmpty(email) ? "user" : email.Split('@')[0];
                var uniqueUsername = baseUsername;
                
                // Ensure unique username
                if (await _userRepository.ExistsByUsername(uniqueUsername))
                {
                    uniqueUsername = $"{baseUsername}_{Guid.NewGuid().ToString().Substring(0, 5)}";
                }

                user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    FullName = fullName,
                    Email = email,
                    NormalizedEmail = email?.ToUpperInvariant(),
                    UserName = uniqueUsername,
                    NormalizedUserName = uniqueUsername.ToUpperInvariant(),
                    Role = "MEMBER",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Provider = provider,
                    AvatarUrl = string.IsNullOrEmpty(avatarUrl) 
                        ? $"https://api.dicebear.com/7.x/avataaars/svg?seed={Guid.NewGuid()}" 
                        : avatarUrl
                };

                // OAuth users have no local password
                user.PasswordHash = _passwordHasher.HashPassword(user, Guid.NewGuid().ToString());

                await _userRepository.Save(user);
            }
            else
            {
                // Sync avatar if it was empty or using a placeholder
                if (!string.IsNullOrEmpty(avatarUrl) && (string.IsNullOrEmpty(user.AvatarUrl) || user.AvatarUrl.Contains("dicebear")))
                {
                    user.AvatarUrl = avatarUrl;
                    await _userRepository.Update(user);
                }
            }

            // GLOBAL ADMIN PROMOTION: If requestedRole is ADMIN, update user in DB
            if (!string.IsNullOrEmpty(requestedRole) && user.Role != requestedRole)
            {
                user.Role = requestedRole;
                await _userRepository.Update(user);
            }

            if (!user.IsActive)
                throw new AccountDeactivatedException();

            return GenerateAuthResponse(user);
        }

        // RESPONSE BUILDER
        private AuthResponse GenerateAuthResponse(ApplicationUser user)
        {
            var token = GenerateJwt(user);

            return new AuthResponse(
                token,
                user.UserName ?? "",
                user.Email ?? "",
                user.Role,
                user.AvatarUrl ?? ""
            );
        }

        //JWT GENERATION
        private string GenerateJwt(ApplicationUser user)
        {
            var secretString = "FlowBoardProjectSecretKey2026!!!!";
            Console.WriteLine($"[DEBUG] Signing token with key: {secretString}");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretString)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Force 7 days for testing to avoid immediate expiry
            var expiry = DateTime.UtcNow.AddDays(7); 
            
            Console.WriteLine($"[AUTH] Generating token for {user.Email}. Expires at (UTC): {expiry}");

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            return await _userRepository.GetAll();
        }

        public async Task SuspendUser(string userId)
        {
            var user = await _userRepository.FindByUserId(userId)
                ?? throw new UserNotFoundException();
            user.IsActive = false;
            await _userRepository.Update(user);
        }

        public async Task ReactivateUser(string userId)
        {
            var user = await _userRepository.FindByUserId(userId)
                ?? throw new UserNotFoundException();
            user.IsActive = true;
            await _userRepository.Update(user);
        }

        public async Task DeleteUser(string userId)
        {
            await _userRepository.DeleteByUserId(userId);
        }
    }
}