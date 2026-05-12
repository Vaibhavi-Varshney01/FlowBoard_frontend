using FlowBoard.Auth.Data;
using FlowBoard.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Auth.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> FindByEmail(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<ApplicationUser?> FindByUsername(string username)
            => await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        public async Task<ApplicationUser?> FindByUserId(string id)
            => await _context.Users.FindAsync(id);

        public async Task<bool> ExistsByEmail(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);

        public async Task<bool> ExistsByUsername(string username)
            => await _context.Users.AnyAsync(u => u.UserName == username);

        public async Task<List<ApplicationUser>> FindAllByRole(string role)
            => await _context.Users.Where(u => u.Role == role).ToListAsync();

        public async Task<ApplicationUser> Save(ApplicationUser user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<ApplicationUser> Update(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteByUserId(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ApplicationUser>> SearchByFullName(string query)
            => await _context.Users
                .Where(u => u.FullName.Contains(query))
                .ToListAsync();

        public async Task<List<ApplicationUser>> GetAll()
            => await _context.Users.ToListAsync();
    }
}