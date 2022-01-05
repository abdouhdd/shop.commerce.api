using Microsoft.EntityFrameworkCore;
using shop.commerce.api.Application.Configuration;
using System;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public class ShopContextFactory : IDisposable
    {
        private readonly DbContextOptions<ShopContext> _options;
        private ShopContext shopContext;

        public ShopContextFactory(DbContextOptions<ShopContext> options)
        {
            _options = options;
            shopContext = new ShopContext(_options);
        }

        public ShopContextFactory(IApplicationSecretsAccessor applicationSecretsAccessor)
        {
            if (applicationSecretsAccessor.GetIsMemoryDatabase())
            {
                _options = new DbContextOptionsBuilder<ShopContext>().UseInMemoryDatabase(applicationSecretsAccessor.GetMemoryDatabase()).Options;
            }
            else if (applicationSecretsAccessor.GetIsSqlServer())
            {
                _options = new DbContextOptionsBuilder<ShopContext>().UseSqlServer(applicationSecretsAccessor.GetMainDatabase()).Options;
            }
            else if (applicationSecretsAccessor.GetIsPGSQL())
            {
                _options = new DbContextOptionsBuilder<ShopContext>().UseNpgsql(applicationSecretsAccessor.GetMainDatabase(), opt => opt.SetPostgresVersion(new Version(9, 6))).Options;
            }
            shopContext = new ShopContext(_options);
            if (applicationSecretsAccessor.GetIsMemoryDatabase())
            {
                //DataList dataList = new DataList();
                //if (shopContext.Admins.Count() == 0 && shopContext.Users.Count() == 0 && shopContext.Categories.Count() == 0
                //    && shopContext.Products.Count() == 0 && shopContext.ProductImages.Count() == 0)
                //{
                //    shopContext.Admins.AddRange(dataList.Admins);
                //    shopContext.SaveChanges();
                //    shopContext.Users.AddRange(dataList.Users);
                //    shopContext.SaveChanges();
                //    shopContext.Categories.AddRange(dataList.Categories);
                //    shopContext.SaveChanges();
                //    shopContext.Products.AddRange(dataList.Products);
                //    shopContext.SaveChanges();
                //    shopContext.ProductImages.AddRange(dataList.ProductImages);
                //    shopContext.SaveChanges();
                //}
            }
        }

        public void Dispose()
        {
            shopContext?.Dispose();
        }

        public ShopContext GetShopContext()
        {
            return shopContext;
        }
    }
}
