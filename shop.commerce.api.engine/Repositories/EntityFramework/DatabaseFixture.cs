using Microsoft.EntityFrameworkCore;
using shop.commerce.api.domain.Entities;
using System;
using System.Collections.Generic;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public class DatabaseFixture : IDisposable
    {
        public readonly ShopContextFactory ContextFactory;
        readonly DbContextOptions<ShopContext> _options;

        public DatabaseFixture(ShopContextFactory shopContextFactory)
        {
            //_options = new DbContextOptionsBuilder<ShopContext>().UseInMemoryDatabase(databaseName).Options;
            //ContextFactory = new ShopContextFactory(_options);
            ContextFactory = shopContextFactory;

            var categories = new List<Category>()
            {
                new Category() { Id = 1, Name = "Category1" },
                new Category() { Id = 2, Name = "Category2" },
                new Category() { Id = 3, Name = "Category3" }
            };


            using (var blogContext = ContextFactory.GetShopContext())
            {
                blogContext.Set<Category>().AddRange(categories);
                blogContext.SaveChanges();
            }
        }

        public void Dispose()
        {
            using (var blogContext = ContextFactory.GetShopContext())
            {
                blogContext.RemoveRange(blogContext.Categories);
                blogContext.SaveChanges();
            }
        }
    }
}
