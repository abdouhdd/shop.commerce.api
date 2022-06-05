

using Microsoft.Extensions.Logging;
using shop.commerce.api.common;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.domain.Entities;
using shop.commerce.api.services.Helpers;
using shop.commerce.api.services.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;
        private readonly MessagesHelper _messagesHelper;

        public CategoryService(ICategoryRepository dataRepository, ILogger<CategoryService> logger, MessagesHelper messagesHelper)
        {
            _categoryRepository = dataRepository;
            _logger = logger;
            _messagesHelper = messagesHelper;
        }

        public MyResult<int> CalculProducts(DataUser dataUser)
        {
            try
            {
                List<Category> categories = _categoryRepository.GetAllCategories();
                if (categories.All(c => c.CountProducts == 0))
                {
                    foreach (Category category in categories)
                    {
                        int countProducts = _categoryRepository.CategoryCountProducts(category.Id);
                        category.CountProducts = countProducts;
                    }
                }

                Result result = _categoryRepository.UpdateRange(categories);
                int output = result.Success ? 1 : 0;
                return MyResult<int>.ResultSuccess(output);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<int>.ResultError(0, ex);
            }
        }

        public MyResult<CategoryView> CreateCategory(CategoryRequest categoryRequest, DataUser dataUser)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryRequest?.Name))
                {
                    return MyResult<CategoryView>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.CategoryObligatory), MyResultCode.CategoryObligatory);
                }
                else
                {
                    categoryRequest.Name = categoryRequest.Name.Trim();
                    if (categoryRequest.Id > 0)
                    {
                        return UpdateCategory(categoryRequest, dataUser);
                    }
                    List<Category> categories = _categoryRepository.GetAllCategories();
                    int position = 0;
                    if (categories.Count > 0)
                    {
                        OrderCategories(categories);
                        position = categories.Max(c => c.Position);
                    }
                    position++;
                    Category category = categories.Find(c => c.Name.ToUpper() == categoryRequest.Name.ToUpper());
                    if (category != null)
                    {
                        return MyResult<CategoryView>.ResultError(new CategoryView
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Active = category.Active,
                            Slug = category.Slug,
                            Level = category.Level,
                            HasChildren = category.HasChildren,
                            Icon = category.Icon,
                            CountProducts = category.CountProducts,
                            ParentId = category.ParentId,
                            Position = category.Position
                        }, _messagesHelper.GetMessageCode(MyResultCode.CategoryAlreadyExist), MyResultCode.CategoryAlreadyExist);
                    }
                    else
                    {
                        Category parent = null;
                        if (categoryRequest.ParentId > 0)
                        {
                            parent = categories.Find(c => c.Id == categoryRequest.ParentId);
                        }
                        category = new Category
                        {
                            Id = 0,
                            Active = categoryRequest.Active,
                            Name = categoryRequest.Name,
                            Slug = categoryRequest.Name,
                            ParentId = parent?.Id,
                            Parent = parent,
                            Level = (parent?.Level ?? 0) + 1,
                            HasChildren = false,
                            Icon = categoryRequest.Icon,
                            CountProducts = 0,
                            Position = position
                        };

                        //int output = _categoryRepository.InsertCategory(category);
                        //_categoryRepository.Save();

                        common.Result<Category> result = _categoryRepository.Add(category);


                        if (parent != null && parent.HasChildren == false)
                        {
                            //parent.HasChildren = true;
                            //output = _categoryRepository.Save();

                            _categoryRepository.Update(parent.Id, (c) =>
                            {
                                c.HasChildren = true;
                            });
                        }

                        Category cat = _categoryRepository.GetById(category.Id);
                        string code = cat != null ? MyResultCode.CategoryInsertedSuccess : MyResultCode.CategoryInsertedError;
                        return new MyResult<CategoryView>
                        {
                            Data = cat != null ? new CategoryView
                            {
                                Id = cat.Id,
                                Name = cat.Name,
                                Active = cat.Active,
                                Slug = cat.Slug,
                                Level = cat.Level,
                                HasChildren = cat.HasChildren,
                                Icon = cat.Icon,
                                CountProducts = cat.CountProducts,
                                ParentId = cat.ParentId,
                                Position = cat.Position
                            } : null,
                            Success = cat != null,
                            Code = code,
                            Message = _messagesHelper.GetMessageCode(code)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<CategoryView>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.CategoryInsertedError), MyResultCode.CategoryInsertedError);
            }
        }

        private void OrderCategories(List<Category> categories)
        {
            try
            {
                int position = 0;
                List<Category> categoriesToUpdate = new List<Category>();
                foreach (Category item in categories)
                {
                    position++;
                    if (item.Position != position)
                    {
                        item.Position = position;
                        categoriesToUpdate.Add(item);
                    }
                }
                _categoryRepository.UpdateRange(categoriesToUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public List<CategoryView> GetAllCategoriesView()
        {
            try
            {
                return _categoryRepository.GetAllCategoriesView();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public List<CategoryView> GetCategoriesView(bool active, bool hasProducts)
        {
            try
            {
                var categories = _categoryRepository.GetAllCategories();
                List<CategoryView> categoryViews = new List<CategoryView>();
                int level = 1;
                CategoryView[] parents = categories.Where(c => c.ParentId == null && (!hasProducts || c.CountProducts > 0 || c.CountAllProducts > 0)).Select(c => new CategoryView
                {
                    Id = c.Id,
                    Name = c.Name,
                    Active = c.Active,
                    Slug = c.Slug,
                    Level = c.Level,
                    HasChildren = c.HasChildren,
                    Icon = c.Icon,
                    CountProducts = c.CountProducts,
                    CountAllProducts = c.CountAllProducts,
                    Position = c.Position,
                    ParentId = c.ParentId
                }).ToArray();

                var list = new List<CategoryView>();
                list.AddRange(parents.Where(c => c.Level != level).ToList());
                parents.Where(c => c.Level != level).ToList().ForEach(c => c.Level = level);

                categoryViews.AddRange(parents);
                getChildren(parents, categories, hasProducts, level, list);

                if (list.Count > 0)
                {
                    UpdateLevelCategories(list, categories);
                    //int output = _categoryRepository.Save();
                }

                return categoryViews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        private void UpdateLevelCategories(List<CategoryView> list, List<Category> categories)
        {
            try
            {
                List<Category> categoriesToUpdate = new List<Category>();
                foreach (CategoryView item in list)
                {
                    Category category = categories.Find(c => c.Id == item.Id);
                    if (category.Level != item.Level)
                    {
                        category.Level = item.Level;
                        categoriesToUpdate.Add(category);
                    }
                }
                if (categoriesToUpdate.Count > 0)
                {
                    _categoryRepository.UpdateRange(categoriesToUpdate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private CategoryView CategoryToView(Category category)
        {
            return new CategoryView
            {
                Id = category.Id,
                Name = category.Name,
                Active = category.Active,
                Slug = category.Slug,
                Level = category.Level,
                ParentId = category.ParentId,
                HasChildren = category.HasChildren,
                Icon = category.Icon,
                CountProducts = category.CountProducts,
                //Parent = category.Parent != null ? CategoryToView(category.Parent) : null
            };
        }

        private void getChildren(CategoryView[] parents, List<Category> categories, bool hasProducts, int level, List<CategoryView> list)
        {
            level++;
            foreach (var item in parents)
            {
                List<CategoryView> children = categories.Where(c => c.ParentId == item.Id && (!hasProducts || c.CountProducts > 0 || c.CountAllProducts > 0)).Select(c => new CategoryView
                {
                    Id = c.Id,
                    Name = c.Name,
                    Active = c.Active,
                    Slug = c.Slug,
                    Level = c.Level,
                    ParentId = c.ParentId,
                    HasChildren = c.HasChildren,
                    Icon = c.Icon,
                    CountProducts = c.CountProducts,
                    //Parent = c.Parent != null ? CategoryToView(c.Parent) : null,
                }).ToList();

                if (children.Count > 0)
                {
                    item.Children = children.Count > 0 ? children.ToArray() : null; 
                    item.HasChildren = children.Count > 0;

                    list.AddRange(children.Where(c => c.Level != level).ToList());
                    children.Where(c => c.Level != level).ToList().ForEach(c => c.Level = level);

                    getChildren(item.Children, categories, hasProducts, level, list);
                }
            }
        }
        
        //public List<CategoryView> GetCategoriesView()
        //{
        //    return _categoryRepository.GetCategoriesView();
        //}

        public List<CategoryView> GetActiveCategoriesView()
        {
            try
            {
                return _categoryRepository.GetAllCategoriesView().Where(c => c.Active && (c.CountProducts > 0 || c.CountAllProducts > 0)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public CategoryView GetCategoryById(int categoryId)
        {
            try
            {
                return _categoryRepository.GetAllCategoriesView().Find(c => c.Id == categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public MyResult<CategoryView> UpdateCategory(CategoryRequest categoryRequest, DataUser dataUser)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryRequest?.Name))
                {
                    return MyResult<CategoryView>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.CategoryObligatory), MyResultCode.CategoryObligatory);
                }
                else
                {
                    int output = 0;
                    categoryRequest.Name = categoryRequest.Name.Trim();
                    List<Category> categories = _categoryRepository.GetAllCategories();

                    Category category = categories.Find(c => c.Id == categoryRequest.Id);
                    if (category == null)
                    {
                        category = categories.Find(c => c.Name.ToUpper() == categoryRequest.Name.ToUpper());
                    }
                    if (category != null)
                    {
                        OrderCategories(categories);
                        Category parent = categories.Find(c => c.Id == categoryRequest.ParentId);
                        category.Active = categoryRequest.Active;
                        category.Name = categoryRequest.Name;
                        category.Slug = categoryRequest.Name;
                        category.Icon = categoryRequest.Icon;
                        category.CountProducts = _categoryRepository.CategoryCountProducts(category.Id);

                        if (parent != null)
                        {
                            Category[] parents = GetParents(parent, categories);
                            if (parent.ParentId == category.Id)
                            {
                                parent.ParentId = category.ParentId;
                                parent.Level--;
                            }
                            else
                            {
                                bool t = false;
                                int? parentId = null;
                                foreach (Category c in parents)
                                {
                                    int? tparentId = c.ParentId;
                                    if (t || category.Id == c.ParentId)
                                    {
                                        t = true;
                                        c.ParentId = parentId;
                                        c.Level--;
                                    }
                                    parentId = tparentId;
                                }
                            }
                            category.ParentId = parent.Id;
                            category.Level = parent.Level + 1;
                            category.HasChildren = categories.Exists(c => c.ParentId == category.Id);

                            // output = _categoryRepository.Save();

                            Result<Category> result = _categoryRepository.Update(category);

                            if (parent.HasChildren == false)
                            {
                                parent.HasChildren = true;
                                // output = _categoryRepository.Save();

                                result = _categoryRepository.Update(parent);
                            }
                        }
                        else
                        {
                            category.ParentId = null;
                            category.Level = 1;
                            category.HasChildren = categories.Exists(c => c.ParentId == category.Id);

                            // output = _categoryRepository.Save();

                            Result<Category> result = _categoryRepository.Update(category);
                        }

                        // Category cat = _categoryRepository.GetCategory(category.Id);

                        Category cat = _categoryRepository.GetById(category.Id);

                        string code = cat == null ? MyResultCode.ErrorCode : output > 0 ? MyResultCode.CategoryUpdatedSuccess : MyResultCode.CategoryUpdatedError;
                        return new MyResult<CategoryView>
                        {
                            Data = cat != null ? new CategoryView
                            {
                                Id = cat.Id,
                                Name = cat.Name,
                                Active = cat.Active,
                                Slug = cat.Slug,
                                Level = cat.Level,
                                HasChildren = cat.HasChildren
                            } : null,
                            Success = cat != null,
                            Code = code,
                            Message = _messagesHelper.GetMessageCode(code)
                        };
                    }
                    else
                    {
                        return MyResult<CategoryView>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.CategoryObligatory), MyResultCode.CategoryObligatory);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<CategoryView>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.CategoryUpdatedError), MyResultCode.CategoryUpdatedError);
            }
        }

        private Category[] GetParents(Category parent, List<Category> categories)
        {
            List<Category> ret = new List<Category>();
            int? parentId = parent.ParentId;
            while (parentId.HasValue)
            {
                Category cat = categories.Find(c => c.Id == parentId);
                parentId = cat.ParentId;
                if (cat != null)
                {
                    ret.Insert(0, cat); 
                }
            }
            return ret.ToArray();
        }

        private void RemoveCategories(Category[] children, List<Category> categories)
        {
            foreach (Category category in children)
            {
                Category[] cats = categories.Where(c => c.ParentId == category.Id).ToArray();
                if (cats.Length > 0)
                {
                    RemoveCategories(cats, categories);
                }
            }
            //int output = _categoryRepository.RemoveCategories(children);

            Result resultDelete = _categoryRepository.DeleteRange(children);
            if (resultDelete.Success)
            {
                categories.RemoveAll(c => children.Contains(c)); 
            }
        }
        
        private bool RemoveCategory(Category category, List<Category> categories)
        {
            List<Category> children = categories.Where(c => c.ParentId == category.Id).ToList();
            if (children.Count > 0)
            {
                foreach (Category cat in children)
                {
                    RemoveCategory(cat, categories);
                }
                categories.RemoveAll(c => children.Contains(c));
            }
            List<Product> products = _categoryRepository.GetProductsCategory(category.Id);
            if (products.Count == 0)
            {
                //int output = _categoryRepository.RemoveCategory(category);
                Result result = _categoryRepository.Delete(category);
                if (result.Success)
                {
                    categories.Remove(category); 
                }
                return result.Success;
            }
            else
            {
                return false;
            }
        }

        public MyResult<CategoryView> RemoveCategory(int categoryId, DataUser dataUser)
        {
            try
            {
                List<Category> categories = _categoryRepository.GetAllCategories();
                Category category = categories.Find(c => c.Id == categoryId);
                bool ok = false;
                if (category != null)
                {
                    if (RemoveCategory(category, categories))
                    {
                        ok = true;
                    }
                    //Category[] children = categories.Where(c => c.ParentId == category.Id).ToArray();
                    //if (children.Length > 0)
                    //{
                    //    RemoveCategories(children, categories);
                    //}
                    //_categoryRepository.RemoveCategory(category);
                }
                string code = category != null && ok ? MyResultCode.CategoryDeletedSuccess : MyResultCode.CategoryDeletedFailed;
                return new MyResult<CategoryView>
                {
                    Data = category != null ? new CategoryView
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Active = category.Active,
                        Slug = category.Slug,
                        Level = category.Level,
                        HasChildren = category.HasChildren,
                        Icon = category.Icon,
                        CountProducts = category.CountProducts,
                        ParentId = category.ParentId
                    } : null,
                    Success = ok,
                    Code = code,
                    Message = _messagesHelper.GetMessageCode(code)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<CategoryView>.ResultError(null, ex);
            }
        }

        public MyResult<bool> CanRemoveCategory(int categoryId, DataUser dataUser)
        {
            try
            {
                Category category = _categoryRepository.GetById(categoryId);
                bool canRemove = false;
                if (category != null)
                {
                    bool hasProducts = _categoryRepository.CategoryCountProducts(categoryId) > 0;
                    canRemove = hasProducts == false;
                }
                return MyResult<bool>.ResultSuccess(canRemove);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<bool>.ResultError(false, ex);
            }
        }

        public List<CategoryView> GetChildrenCategory(int? categoryId, bool active, bool hasProducts)
        {
            try
            {
                List<CategoryView> list = _categoryRepository.GetChildrenCategory(categoryId, active, hasProducts).Select(c => new CategoryView
                {
                    Id = c.Id,
                    Name = c.Name,
                    Active = c.Active,
                    Slug = c.Slug,
                    Level = c.Level,
                    HasChildren = c.HasChildren,
                    Icon = c.Icon,
                    CountProducts = c.CountProducts,
                    ParentId = c.ParentId
                }).ToList();
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }
    }
}
