using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.services.Models;
using System.Collections.Generic;

namespace shop.commerce.api.services.Services
{
    public interface ICategoryService
    {
        List<CategoryView> GetAllCategoriesView();
        List<CategoryView> GetChildrenCategory(int? categoryId, bool active, bool hasProducts);
        List<CategoryView> GetCategoriesView(bool active, bool hasProducts);
        MyResult<CategoryView> CreateCategory(CategoryRequest categoryRequest, DataUser dataUser);
        MyResult<CategoryView> UpdateCategory(CategoryRequest categoryRequest, DataUser dataUser);
        MyResult<CategoryView> RemoveCategory(int categoryId, DataUser dataUser);
        MyResult<bool> CanRemoveCategory(int categoryId, DataUser dataUser);
        CategoryView GetCategoryById(int categoryId);
        List<CategoryView> GetActiveCategoriesView();
        MyResult<int> CalculProducts(DataUser dataUser);
    }
}
