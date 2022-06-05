using shop.commerce.api.domain.Views;
using shop.commerce.api.domain.Entities;
using System;
using System.Collections.Generic;

namespace shop.commerce.api.infrastructure.Repositories
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        List<Category> GetAllCategories();
        List<CategoryView> GetAllCategoriesView();
        //int InsertCategory(Category category);
        //Category GetCategory(int categoryId);
        //Category RemoveCategory(int categoryId);
        int CategoryCountProducts(int? categoryId);
        //int RemoveCategory(Category category);
        //int RemoveCategories(params Category[] categories);
        List<Product> GetProductsCategory(int categoryId);
        List<Category> GetChildrenCategory(int? categoryId, bool active, bool hasProducts);
        int[] GetAllChildrenCategory(int? categoryId, bool active);
        int[] GetAllChildrenCategories(int[] categories, bool active);
        void ForChildren(Category[] parents, List<Category> categories, bool hasProducts, int level, Action<Category, Category[], int> handle);
    }
}
