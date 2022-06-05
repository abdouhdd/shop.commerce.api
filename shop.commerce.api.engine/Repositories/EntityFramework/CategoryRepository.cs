using Microsoft.Extensions.Logging;
using shop.commerce.api.domain.Views;
using shop.commerce.api.domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class CategoryRepository : ICategoryRepository
    {
        public List<Category> GetAllCategories()
        {
            return _context.Categories.OrderBy(c => c.Position).ToList();
        }

        public List<CategoryView> GetAllCategoriesView()
        {
            return _entity.OrderBy(c => c.Position).Select(c => new CategoryView
            {
                Id = c.Id,
                Name = c.Name,
                Active = c.Active,
                Slug = c.Slug,
                CountProducts = c.CountProducts,
                CountAllProducts = c.CountAllProducts,
                HasChildren = c.HasChildren,
                Position = c.Position,
                Icon = c.Icon,
                Level = c.Level,
                ParentId = c.ParentId,
            }).ToList();
        }
        
        public List<Category> GetChildrenCategory(int? categoryId, bool active, bool hasProducts)
        {
            return _entity.OrderBy(c => c.Position).Where(c => c.ParentId == categoryId && c.Active == active && (!hasProducts || c.CountProducts > 0 || c.CountAllProducts > 0)).ToList();
        }
        
        public int[] GetAllChildrenCategory(int? categoryId, bool active)
        {
            return _entity.OrderBy(c => c.Position).Where(c => c.ParentId == categoryId && c.Active == active).Select(c => c.Id).ToArray();
        }

        public int[] GetAllChildrenCategories(int[] categories, bool active)
        {
            int[] tab = (int[])categories.Clone();
            foreach (int categoryId in categories)
            {
                int[] t = GetAllChildrenCategory(categoryId, active);
                if (t.Length > 0)
                {
                    tab = tab.Concat(t).ToArray();
                    t = GetAllChildrenCategories(t, active);
                    if (t.Length > 0)
                    {
                        tab = tab.Concat(t).ToArray();
                    }
                }
            }
            return tab;
        }

        //public Category GetCategory(int categoryId)
        //{
        //    return _entity.AsEnumerable().SingleOrDefault(c => c.Id == categoryId);
        //}

        //public int InsertCategory(Category category)
        //{
        //    _entity.Add(category);
        //    return 0;
        //}

        //public Category RemoveCategory(int categoryId)
        //{
        //    Category category = _entity.Find(categoryId);
        //    if (category != null)
        //    {
        //        _entity.Remove(category);
        //        _context.SaveChanges();
        //    }
        //    return category;
        //}
        
        //public int RemoveCategory(Category category)
        //{
        //    _entity.Remove(category);
        //    return _context.SaveChanges();
        //}
        
        //public int RemoveCategories(params Category[] categories)
        //{
        //    _entity.RemoveRange(categories);
        //    return _context.SaveChanges();
        //}
        
        public int CategoryCountProducts(int? categoryId)
        {
            int count = _context.Products.Count(p => p.CategoryId == categoryId);
            return count;
        }

        public List<Product> GetProductsCategory(int categoryId)
        {
            return _context.Products.Where(p => p.CategoryId == categoryId).ToList();
        }

        public void ForChildren(Category[] parents, List<Category> categories, bool hasProducts, int level, Action<Category, Category[], int> handle)
        {
            level++;
            foreach (var item in parents)
            {
                List<Category> children = categories.Where(c => c.ParentId == item.Id && (!hasProducts || c.CountProducts > 0 || c.CountAllProducts > 0)).ToList();

                if (children.Count > 0)
                {
                    item.Children = children.Count > 0 ? children.ToArray() : null;
                    item.HasChildren = children.Count > 0;

                    ForChildren((Category[])item.Children, categories, hasProducts, level, handle);
                }
                else
                {
                    item.Children = null;
                    item.HasChildren = false;
                }
                handle(item, (Category[])item.Children, level);
            }
        }
    }

    public partial class CategoryRepository : Repository<Category, int>
    {
        //private readonly ShopContext _context;

        public CategoryRepository(ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory)
        {
        }
    }
}
