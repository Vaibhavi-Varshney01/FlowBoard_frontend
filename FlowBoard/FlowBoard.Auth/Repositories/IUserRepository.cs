using FlowBoard.Auth.Models;

namespace FlowBoard.Auth.Repositories
{
    public interface IUserRepository
    {
    Task<ApplicationUser?> FindByEmail(string email);
    Task<ApplicationUser?> FindByUsername(string username);
    Task<ApplicationUser?> FindByUserId(string id);
    Task DeleteByUserId(string id);

    Task<bool> ExistsByEmail(string email);
    Task<bool> ExistsByUsername(string username);

    Task<List<ApplicationUser>> FindAllByRole(string role);
    Task<List<ApplicationUser>> SearchByFullName(string query);
    Task<List<ApplicationUser>> GetAll();

    Task<ApplicationUser> Save(ApplicationUser user);
    Task<ApplicationUser> Update(ApplicationUser user);
    // Task DeleteByUserId(string id);
    }
}