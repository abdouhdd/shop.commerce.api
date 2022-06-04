using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class UserRepository : IUserRepository
    {
        public User FindUserByEmail(string email)
            => _context.Users.SingleOrDefault(a => a.Email == email);

        public User FindUserByUsername(string username)
            => _context.Users.SingleOrDefault(a => a.Username == username);

        public IEnumerable<User> FindUsers()
            => _context.Users.ToList();

        public int SaveUsers(User[] users)
        {
            foreach (User user in users)
            {
                _context.Users.Add(user);
            }
            return _context.SaveChanges();
        }

        public bool EnsureDeleted()
        {
            bool deleted = _context.Database.EnsureDeleted();
            return deleted;
        }
    }

    public partial class UserRepository : Repository<User, int>
    {
        //private readonly ShopContext _context;

        //public AccountRepositoryEF(ShopContextFactory shopContextFactory)
        //{
        //    this._context = shopContextFactory.GetShopContext();
        //}

        public UserRepository(
            ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory) { }
    }
}
