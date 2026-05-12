using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using FlowBoard.Auth.Services;
using FlowBoard.Auth.Models;
using FlowBoard.Auth.Repositories;
using FlowBoard.Auth.DTOs;
using FlowBoard.Auth.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FlowBoard.Tests.Auth
{
    [TestFixture]
    public class AuthServiceTests
    {
        private IUserRepository                  _userRepo;
        private IPasswordHasher<ApplicationUser> _passwordHasher;
        private IOptions<JwtSettings>            _jwtOptions;
        private IAuthService                     _sut;

        [SetUp]
        public void SetUp()
        {
            _userRepo       = Substitute.For<IUserRepository>();
            _passwordHasher = Substitute.For<IPasswordHasher<ApplicationUser>>();
            _jwtOptions     = Substitute.For<IOptions<JwtSettings>>();

            _jwtOptions.Value.Returns(new JwtSettings
            {
                Secret      = "SuperSecretTestKey1234567890ABCDEF",
                Issuer      = "FlowBoard",
                Audience    = "FlowBoardUsers",
                ExpiryHours = 24
            });

            _passwordHasher
                .HashPassword(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns("hashed_password");

            _sut = new AuthServiceImpl(_userRepo, _jwtOptions, _passwordHasher);
        }

        // ── Register ──────────────────────────────────────────────────────────

        [Test]
        public async Task Register_WithValidRequest_ReturnsAuthResponse()
        {
            var request = new RegisterRequest
            {
                FullName = "Test User",
                Username = "testuser",
                Email    = "test@example.com",
                Password = "Password123!"
            };

            _userRepo.ExistsByEmail(request.Email).Returns(false);
            _userRepo.ExistsByUsername(request.Username).Returns(false);
            _userRepo.Save(Arg.Any<ApplicationUser>()).Returns(ci =>
            {
                var u = ci.Arg<ApplicationUser>();
                u.Id = "user-123";
                return u;
            });

            var result = await _sut.Register(request);

            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(request.Email);
        }

        [Test]
        public async Task Register_WithDuplicateEmail_ThrowsException()
        {
            var request = new RegisterRequest
            {
                FullName = "Test User",
                Username = "testuser",
                Email    = "duplicate@example.com",
                Password = "Password123!"
            };

            _userRepo.ExistsByEmail(request.Email).Returns(true);

            var act = async () => await _sut.Register(request);

            await act.Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task Register_WithDuplicateUsername_ThrowsException()
        {
            var request = new RegisterRequest
            {
                FullName = "Test User",
                Username = "existinguser",
                Email    = "new@example.com",
                Password = "Password123!"
            };

            _userRepo.ExistsByEmail(request.Email).Returns(false);
            _userRepo.ExistsByUsername(request.Username).Returns(true);

            var act = async () => await _sut.Register(request);

            await act.Should().ThrowAsync<Exception>();
        }

        // ── Login ─────────────────────────────────────────────────────────────

        [Test]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var user = new ApplicationUser
            {
                Id           = "user-123",
                Email        = "test@example.com",
                FullName     = "Test User",
                IsActive     = true,
                PasswordHash = "hashed_password"
            };
            var request = new LoginRequest { Email = "test@example.com", Password = "Password123!" };

            _userRepo.FindByEmail(request.Email).Returns(user);
            _passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, request.Password)
                .Returns(PasswordVerificationResult.Success);

            var result = await _sut.Login(request);

            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(user.Email);
        }

        [Test]
        public async Task Login_WithInvalidPassword_ThrowsException()
        {
            var user    = new ApplicationUser { Email = "test@example.com", IsActive = true, PasswordHash = "hashed_password" };
            var request = new LoginRequest   { Email = "test@example.com", Password = "WrongPassword" };

            _userRepo.FindByEmail(request.Email).Returns(user);
            _passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, request.Password)
                .Returns(PasswordVerificationResult.Failed);

            var act = async () => await _sut.Login(request);

            await act.Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task Login_WithNonExistentEmail_ThrowsException()
        {
            var request = new LoginRequest { Email = "nobody@example.com", Password = "Password123!" };
            _userRepo.FindByEmail(request.Email).Returns((ApplicationUser?)null);

            var act = async () => await _sut.Login(request);

            await act.Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task Login_WithDeactivatedAccount_ThrowsException()
        {
            var user    = new ApplicationUser { Email = "test@example.com", IsActive = false, PasswordHash = "hashed_password" };
            var request = new LoginRequest   { Email = "test@example.com", Password = "Password123!" };

            _userRepo.FindByEmail(request.Email).Returns(user);

            var act = async () => await _sut.Login(request);

            await act.Should().ThrowAsync<Exception>();
        }

        // ── GetUserByEmail ────────────────────────────────────────────────────

        [Test]
        public async Task GetUserByEmail_WhenExists_ReturnsUser()
        {
            var user = new ApplicationUser { Email = "test@example.com", FullName = "Test User" };
            _userRepo.FindByEmail("test@example.com").Returns(user);

            var result = await _sut.GetUserByEmail("test@example.com");

            result.Should().NotBeNull();
            result.Email.Should().Be("test@example.com");
        }

        [Test]
        public async Task GetUserByEmail_WhenNotExists_ThrowsException()
        {
            _userRepo.FindByEmail("nobody@example.com").Returns((ApplicationUser?)null);

            var act = async () => await _sut.GetUserByEmail("nobody@example.com");

            await act.Should().ThrowAsync<Exception>();
        }

        // ── SearchUsers ───────────────────────────────────────────────────────

        [Test]
        public async Task SearchUsers_ReturnsMatchingUsers()
        {
            var users = new List<ApplicationUser>
            {
                new() { FullName = "Alice Smith", Email = "alice@example.com" },
                new() { FullName = "Alice Jones", Email = "alice2@example.com" }
            };

            _userRepo.SearchByFullName("Alice").Returns(users);

            var result = await _sut.SearchUsers("Alice");

            result.Should().HaveCount(2);
            result.All(u => u.FullName.Contains("Alice")).Should().BeTrue();
        }

        // ── DeactivateAccount ─────────────────────────────────────────────────

        [Test]
        public async Task DeactivateAccount_SetsIsActiveToFalse()
        {
            var user = new ApplicationUser { Id = "user-123", IsActive = true };
            _userRepo.FindByUserId("user-123").Returns(user);
            _userRepo.Update(Arg.Any<ApplicationUser>()).Returns(ci => ci.Arg<ApplicationUser>());

            await _sut.DeactivateAccount("user-123");

            await _userRepo.Received(1).Update(Arg.Is<ApplicationUser>(u => u.IsActive == false));
        }
    }
}