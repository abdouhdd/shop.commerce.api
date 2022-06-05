using shop.commerce.api.domain.Entities;
using System.Collections.Generic;

namespace shop.commerce.api.infrastructure.Repositories
{
    public interface IUserRepository : IRepository<User, int>
    {
        bool EnsureCreated();
        User FindUserByEmail(string email);
        User FindUserByUsername(string username);
        IEnumerable<User> FindUsers();
        bool EnsureDeleted();
        int SaveUsers(User[] users);
    }
}
