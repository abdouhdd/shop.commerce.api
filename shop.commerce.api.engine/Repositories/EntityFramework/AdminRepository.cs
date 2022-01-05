using Microsoft.Extensions.Logging;
using shop.commerce.api.infrastructure.Repositories.Entities;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class AdminRepository : IAdminRepository
    {
        
        public Admin FindAdminByEmail(string email)
            => _context.Admins.AsEnumerable().SingleOrDefault(a => a.Email == email);

        public Admin FindAdminByUsername(string username)
            => _context.Admins.SingleOrDefault(a => a.Username == username);

        public IEnumerable<Admin> FindAdmins()
            => _context.Admins.ToList();

        public int SaveAdmins(Admin[] admins)
        {
            foreach (Admin admin in admins)
            {
                _context.Admins.Add(admin);
            }
            return _context.SaveChanges();
        }

    }

    public partial class AdminRepository : Repository<Admin, int>
    {
        //private readonly ShopContext _context;

        //public AccountRepositoryEF(ShopContextFactory shopContextFactory)
        //{
        //    this._context = shopContextFactory.GetShopContext();
        //}

        public AdminRepository(
            ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory) { }
    }
}
