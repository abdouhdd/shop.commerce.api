using shop.commerce.api.infrastructure.Repositories.Entities;
using System.Collections.Generic;

namespace shop.commerce.api.infrastructure.Repositories
{
    public interface IAdminRepository : IRepository<Admin, int>
    {
        Admin FindAdminByEmail(string email);
        Admin FindAdminByUsername(string username);
        IEnumerable<Admin> FindAdmins();
        int SaveAdmins(Admin[] admins);
    }
}
