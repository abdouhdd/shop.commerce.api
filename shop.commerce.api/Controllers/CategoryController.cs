using shop.commerce.api.services.Services;
using Microsoft.AspNetCore.Mvc;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Views;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace shop.commerce.api.presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerCore
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize]
        [HttpPost("CreateCategory")]
        public ActionResult CreateCategory(CategoryRequest categoryRequest)
        {
            MyResult<CategoryView> result = _categoryService.CreateCategory(categoryRequest, DataUser);
            return ActionResultFor(result);
        }

        [Authorize]
        [HttpPost("CalculProducts")]
        public ActionResult CalculProducts()
        {
            return ActionResultFor(_categoryService.CalculProducts(DataUser));
        }

        [HttpPost("GetAllCategoriesView")]
        public ActionResult GetAllCategoriesView()
        {
            List<CategoryView> result = _categoryService.GetAllCategoriesView();
            return Ok(result);
        }
        
        [HttpPost("GetChildrenCategory")]
        public ActionResult GetChildrenCategory(int? categoryId, bool active=false, bool hasProducts=false)
        {
            List<CategoryView> result = _categoryService.GetChildrenCategory(categoryId, active, hasProducts);
            return Ok(result);
        }

        [HttpPost("GetCategoriesView")]
        public ActionResult GetCategoriesView(bool active=true, bool hasProducts=false)
        {
            List<CategoryView> result = _categoryService.GetCategoriesView(active, hasProducts);
            return Ok(result);
        }
        
        [HttpPost("GetActiveCategoriesView")]
        public ActionResult GetActiveCategoriesView()
        {
            List<CategoryView> result = _categoryService.GetActiveCategoriesView();
            return Ok(result);
        }
        
        [HttpPost("GetCategory")]
        public ActionResult GetCategory(int categoryId)
        {
            CategoryView result = _categoryService.GetCategoryById(categoryId);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("UpdateCategory")]
        public ActionResult UpdateCategory(CategoryRequest categoryRequest)
        {
            MyResult<CategoryView> result = _categoryService.UpdateCategory(categoryRequest, DataUser);
            return ActionResultFor(result);
        }
        
        [Authorize]
        [HttpPost("RemoveCategory")]
        public ActionResult RemoveCategory(int categoryId)
        {
            MyResult<CategoryView> result = _categoryService.RemoveCategory(categoryId, DataUser);
            return ActionResultFor(result);
        }
        
        [HttpPost("CanRemoveCategory")]
        public ActionResult CanRemoveCategory(int categoryId)
        {
            MyResult<bool> result = _categoryService.CanRemoveCategory(categoryId, DataUser);
            return ActionResultFor(result);
        }
    }
}
