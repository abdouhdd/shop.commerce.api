using shop.commerce.api.Application.Configuration;
using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Extensions;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Account;
using shop.commerce.api.domain.Models.Request;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.services.Models;
using shop.commerce.api.services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace shop.commerce.api.services.Testing
{
    public class InitApp
    {
        private readonly ICategoryService _categoryService;
        private readonly IAccountService _accountService;
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly TestData _testData;
        private DataUser dataUser;
        public InitApp(ICategoryService dataService, IAccountService accountService, IAdminService adminService, IUserService userService, TestData testData, IApplicationSettingsAccessor applicationSettingsAccessor)
        {
            _categoryService = dataService;
            _accountService = accountService;
            _adminService = adminService;
            _userService = userService;
            _testData = testData;

            _adminService.ApplicationSettingsAccessor = applicationSettingsAccessor;
            _userService.ApplicationSettingsAccessor = applicationSettingsAccessor;
        }

        public void Init()
        {
            //CategoriesToJson();
            
            //bool r = _accountService.EnsureDeleted();
            bool b = _accountService.EnsureCreated();

            string password = "123456";
            string password2 = "Tanger@2000";

            Admin[] initadmins = new Admin[]
            {
                new Admin
                {
                    Email = "admin@gmail.com",
                    Username = "admin",
                    PasswordHash = password
                },
                new Admin
                {
                    Email = "saoudnet@gmail.com",
                    Username = "saoudnet",
                    PasswordHash = password2
                },
                new Admin
                {
                    Email = "alfaker.10@gmail.com",
                    Username = "alfaker",
                    PasswordHash = password2
                },
                new Admin
                {
                    Email = "fibronet.ma@gmail.com",
                    Username = "fibronet",
                    PasswordHash = password2
                },
                new Admin
                {
                    Email = "s.saoud.etnt@gmail.com",
                    Username = "saoud",
                    PasswordHash = password2
                },
            };
            
            User[] initUsers = new User[]
            {
                new User
                {
                    Email = "user@gmail.com",
                    Username = "user",
                    PasswordHash = password
                }
            };

            var admins = _accountService.FindAdmins().ToList();
            var newAdmins = initadmins.Where(na => !admins.Exists(a => a.Email == na.Email)).ToArray();
            if (newAdmins.Length > 0)
            {
                _accountService.SaveAdmins(newAdmins);
            }

            var users = _accountService.FindUsers().ToList();
            var newUsers = initUsers.Where(na => !users.Exists(a => a.Email == na.Email)).ToArray();
            if (newUsers.Length > 0)
            {
                _accountService.SaveUsers(newUsers);
            }

            var resultAdmin = _accountService.AuthenticateAdmin(new AuthenticationRequest { Email = initadmins[0].Email, Password = password });
            if (resultAdmin.Success)
            {
                dataUser = new DataUser
                {
                    Username = resultAdmin.Data.UserName,
                    Role = EnumRole.Admin
                };

                var categories = _categoryService.GetAllCategoriesView();
                if (categories.Count == 0)
                {
                    //foreach (var category in _testData.Categories)
                    //{
                    //    _categoryService.CreateCategory(new CategoryRequest
                    //    {
                    //        Name = category.Name,
                    //        Active = category.Active,
                    //    }, dataUser);
                    //}
                    CreateCategories(_testData.SubCategorie);
                }

                //MyResult<int> resultCalcul = _categoryService.CalculProducts(dataUser);

                MyResult<int> resultUpdate = _adminService.UpdateAllSearchTerms(dataUser);
                
                _adminService.UpdateImagesPosition(dataUser);

                
                var categoriesView = _categoryService.GetCategoriesView(active:false, hasProducts:false);

                if (categoriesView.Count > 0)
                {
                    var products = _userService.GetProductsView(new domain.Filters.ProductFilterUser { }, dataUser);

                    if (products.Count == 0)
                    {
                        ProductRequest[] productsRequest = _testData.Products();
                        ProductRequest request = productsRequest[0];
                        int count = 3;
                        List<ProductRequest> productRequests = new List<ProductRequest>();
                        for (int i = 0; i < count; i++)
                        {
                            int index = new Random().Next(0, categoriesView.Count / 2);
                            request.CategoryId = categoriesView[index].Id;
                            do
                            {
                                request.OldPrice = new Random().Next(3, 10000);
                            } while (request.OldPrice > 10 && request.OldPrice % 10 != 0);
                            request.IsOffer = request.OldPrice % 2 == 0;
                            if (request.IsOffer)
                            {
                                do
                                {
                                    request.Offer = new Random().Next(5, 60);
                                } while (request.Offer % 5 != 0);
                                request.NewPrice = Calcul.CalculSpecialPrice(request.Offer, request.OldPrice);

                                decimal offer = Calcul.Offer(request.OldPrice, request.NewPrice);
                                if (offer != request.Offer)
                                {

                                }
                            }
                            productRequests.Add(new ProductRequest
                            {
                                Name = request.Name + " " + (i + 1),
                                CategoryId = request.CategoryId,
                                Description = request.Description,
                                Images = images(),
                                MetaDescription = request.Name,
                                MetaTitle = request.Name,
                                MetaKeywords = request.Name.Replace(" ", ","),
                                Active = request.Active,
                                Specification = request.Specification,
                                Quantity = new Random().Next(0, 100),
                                NewPrice = request.NewPrice,
                                OldPrice = request.OldPrice,
                                IsOffer = request.IsOffer,
                                Offer = request.Offer,
                                Rating = new Random().Next(0, 5),
                            });
                        }
                        productsRequest = productRequests.ToArray();
                        foreach (var productRequest in productsRequest)
                        {
                            var resultProduct = _adminService.CreateProduct(productRequest, dataUser);
                            if (resultProduct != null && resultProduct.Data != null)
                            {
                                _adminService.SaveImages(productRequest.Images, resultProduct.Data.Slug, dataUser);
                            }
                        }

                        products = _userService.GetProductsView(new domain.Filters.ProductFilterUser { }, dataUser);

                        CreateOrders(products, resultAdmin);                    
                    }


                    var resultUser = _accountService.AuthenticateUser(new AuthenticationRequest { Email = initUsers[0].Email, Password = password });
                    if (resultUser.Success)
                    {
                        var productsView = _userService.GetProductsView(new domain.Filters.ProductFilterUser { }, dataUser);
                        foreach (var productView in productsView)
                        {
                            var productDetailView = _userService.GetProductDetailView(productView.Slug, dataUser);
                        }
                    }
                }
            }
        }

        private void CategoriesToJson()
        {
            int i = 1;
            foreach (var c1 in _testData.SubCategorie)
            {
                c1.Id = i++;
                if (c1.Children?.Count > 0)
                {
                    foreach (var c2 in c1.Children)
                    {
                        c2.Id = i++;
                        if (c2.Children?.Count > 0)
                        {
                            foreach (var c3 in c2.Children)
                            {
                                c3.Id = i++;
                                if (c3.Children?.Count > 0)
                                {
                                }
                                else
                                {
                                    c3.Children = null;
                                }
                            }
                        }
                        else
                        {
                            c2.Children = null;
                        }
                    }
                }
                else
                {
                    c1.Children = null;
                }
            }
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(_testData.SubCategorie));
        }

        private void CreateOrders(List<ProductView> products, MyResult<ProfileModel> resultAdmin)
        {
            List<OrderItemRequest> orderProducts = new List<OrderItemRequest>();
            foreach (var product in products.Take(3))
            {
                var productDetail = _userService.GetProductDetailView(product.Slug, dataUser);
                orderProducts.Add(new OrderItemRequest
                {
                    Price = product.NewPrice > 0 ? product.NewPrice : product.OldPrice,
                    Qty = new Random().Next(2, 3),
                    Slug = product.Slug,
                });
            }
            OrderRequest orderRequest = new OrderRequest
            {
                Email = resultAdmin.Data.Email,
                Country = "Maroc",
                FullName = "abderrahman",
                Phone = "+212615546536",
                City = "Tanger",
                OrderStatus = EnumOrderStatus.Pending,
                OrdersNote = "",
                Address = "branes",
                TotalAmount = orderProducts.Sum(p => p.Price),
                TotalQty = orderProducts.Sum(p => p.Qty),
                ZipCode = "",
                PaymentMethod = EnumPaymentMethod.cash_on_delivery,
                Items = orderProducts.ToArray(),
            };

            var resultCreateOrder = _userService.CreateOrder(orderRequest, dataUser);

            //OrderRequest[] orders = _testData.Orders;
            //foreach (var order in orders)
            //{
            //    var resultCreateOrder = _orderService.CreateOrder(order, dataUser);
            //}

            var orders = _adminService.GetOrdersView(new OrderFilterAdmin { }, dataUser).ToList();

            for (int i = 0; i < orders.Count; i++)
            {
                if (i % 2 == 0)
                {
                    string number = orders[i].OrderNumber;
                    var result = _adminService.UpdateOrderStatus(number, EnumOrderStatus.Processing, dataUser);
                    result = _adminService.UpdateOrderStatus(number, EnumOrderStatus.Canceled, dataUser);
                    result = _adminService.UpdateOrderStatus(number, EnumOrderStatus.Processing, dataUser);
                    result = _adminService.UpdateOrderStatus(number, EnumOrderStatus.Completed, dataUser);
                    var orderTracking = _adminService.GetOrderTrackings(number, dataUser);
                }
            }
        }

        private void CreateCategories(List<CategoryRequest> categories, int? parentId=null)
        {
            foreach (var categoryRequest in categories)
            {
                categoryRequest.Active = true;
                categoryRequest.ParentId = parentId;
                var result = _categoryService.CreateCategory(categoryRequest, dataUser);
                if (result.Data?.Id > 0 && categoryRequest.Children?.Count > 0)
                {
                    categoryRequest.Id = result.Data.Id;
                    CreateCategories(categoryRequest.Children, result.Data.Id);
                }
            }
        }

        private string[] images()
        {
            int count = new Random().Next(3, 5);
            string[] imgs = new string[count];
            int i = 0;
            do
            {
                int num = new Random().Next(1, 8);
                string img = $"img-{num}.png";
                if (!imgs.Contains(img))
                {
                    imgs[i] = img;
                    i++;
                }
            } while (i < count);
            return imgs;
        }

        public async Task InitAsync()
        {
            await Task.Run(Init);
        }
    }
}
