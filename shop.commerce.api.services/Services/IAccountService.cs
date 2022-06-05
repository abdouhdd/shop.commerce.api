using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Account;
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.domain.Entities;
using System.Collections.Generic;

namespace shop.commerce.api.services.Services
{
    public interface IAccountService
    {
        Admin FindAdminByEmail(string email);
        Admin FindAdminByUsername(string username);

        IEnumerable<User> FindUsers();

        int SaveUsers(User[] users);
        MyResult<ProfileModel> AuthenticateAdmin(AuthenticationRequest authenticationRequest);
        MyResult<ProfileModel> AuthenticateUser(AuthenticationRequest authenticationRequest);
        int SaveAdmins(Admin[] users);
        IEnumerable<Admin> FindAdmins();
        bool EnsureCreated();
        bool EnsureDeleted();
    }
}
