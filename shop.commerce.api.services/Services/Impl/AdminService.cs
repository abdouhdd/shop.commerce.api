using Microsoft.Extensions.Logging;
using shop.commerce.api.Application.Configuration;
using shop.commerce.api.common;
using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Extensions;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Request;
using shop.commerce.api.domain.Models.Response;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.domain.Entities;
using shop.commerce.api.services.Helpers;
using shop.commerce.api.services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace shop.commerce.api.services.Services
{
    public class AdminService : IAdminService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderTrackingRepository _orderTrackingRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly ILogger<AdminService> _logger;
        private readonly MessagesHelper _messagesHelper;
        public IApplicationSettingsAccessor ApplicationSettingsAccessor { get; set; }

        public AdminService(IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IOrderRepository orderRepository,
            IProductImageRepository productImageRepository,
            IOrderTrackingRepository orderTrackingRepository,
            IAdminRepository adminRepository,
            ILogger<AdminService> logger,
            MessagesHelper messagesHelper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _productImageRepository = productImageRepository;
            _orderTrackingRepository = orderTrackingRepository;
            _adminRepository = adminRepository;
            _logger = logger;
            _messagesHelper = messagesHelper;
        }

        public ProductDetailView GetProductDetailView(string slug, DataUser admin)
        {
            try
            {
                ProductDetailView productDetailView = _productRepository.GetProductDetailView(slug);

                //if (productDetailView != null)
                //{
                //    for (int i = 0; i < productDetailView.Images.Length; i++)
                //    {
                //        productDetailView.Images[i] = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), slug, productDetailView.Images[i]);
                //    }
                //}
                return productDetailView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public List<ProductView> GetProductsView(ProductFilterAdmin productFilter, DataUser admin)
        {
            try
            {
                productFilter.Seller = admin.Username;
                List<ProductView> productViews = _productRepository.GetProductsViewAdmin(productFilter);
                //if (productViews != null)
                //{
                //    foreach (ProductView productView in productViews)
                //    {
                //        productView.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), productView.Slug, productView.Image);
                //    }
                //}
                return productViews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public ResultPage<ProductView> GetProductsViewPage(ProductFilterAdmin productFilter, DataUser seller)
        {
            ResultPage<ProductView> productViews = null;
            try
            {
                // productFilter.Seller = seller.Username;

                int[] categories = null;
                if (productFilter.CategoryId > 0)
                {
                    categories = new int[] { productFilter.CategoryId.Value };
                    categories = _categoryRepository.GetAllChildrenCategories(categories, true);
                }
                else
                {
                    productFilter.CategoryId = null;
                }
                productViews = _productRepository.GetProductsViewAdminPage(productFilter, categories);
                if (productViews.List.All(p => p.Position == 0))
                {
                    OrderProducts(true);
                }
                //if (productViews != null)
                //{
                //    foreach (ProductView productView in productViews.List)
                //    {
                //        productView.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), productView.Slug, productView.Image);
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                productViews = null;
            }
            return productViews;
        }

        public MyResult<bool> OrderProducts(bool force)
        {
            try
            {
                List<Product> products = _productRepository.FindAllProducts();
                bool ok = false;
                int position = 0;
                if (force || _productRepository.AllProducts(p => p.Position == 0))
                {
                    foreach (Product item in products)
                    {
                        item.Position = ++position;
                    }
                    // int output = _productRepository.Save();
                    Result result = _productRepository.UpdateRange(products);
                    ok = result.Success;
                }
                return MyResult<bool>.ResultSuccess(ok);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<bool>.ResultError(false, ex);
            }
        }

        public MyResult<bool> UpProduct(string slug)
        {
            try
            {
                Product product = _productRepository.GetProductBy(slug);
                int output = 0;
                if (product != null)
                {
                    List<Product> products = _productRepository.GetProductByPosition(product.Position, product.Position + 1);
                    Product up = products.Where(p => p.Position == product.Position + 1).FirstOrDefault();
                    if (products.Count > 2)
                    {
                        int i = product.Position + 2;
                        List<Product> upProducts = _productRepository.GetProductByPosition(product.Position + 2);

                        List<Product> productsToUpdate = new List<Product>();
                        foreach (Product item in products.OrderBy(p => p.Position))
                        {
                            if (item != product && item != up)
                            {
                                if (item.Position != i)
                                {
                                    productsToUpdate.Add(item);
                                }
                                item.Position = i++;
                            }
                        }
                        // output = _productRepository.Save();

                        if (productsToUpdate.Count > 0)
                        {
                            Result result = _productRepository.UpdateRange(productsToUpdate);
                            output = result.Success ? productsToUpdate.Count : 0;
                        }
                        productsToUpdate.Clear();
                        foreach (Product item in upProducts.OrderBy(p => p.Position))
                        {
                            if (item.Position != i)
                            {
                                productsToUpdate.Add(item);
                            }
                            item.Position = i++;
                        }
                        // output = _productRepository.Save();

                        if (productsToUpdate.Count > 0)
                        {
                            Result result = _productRepository.UpdateRange(productsToUpdate);
                            output = result.Success ? productsToUpdate.Count : 0;
                        }
                    }
                    if (up != null)
                    {
                        up.Position--;
                        product.Position++;
                        // output = _productRepository.Save();
                        Result result = _productRepository.UpdateRange(new Product[] { up, product });
                    }
                    else
                    {
                        product.Position++;
                        // output = _productRepository.Save();
                        Result result = _productRepository.Update(product);
                    }
                }
                return MyResult<bool>.ResultSuccess(output > 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<bool>.ResultError(false, ex);
            }
        }

        public MyResult<bool> DownProduct(string slug)
        {
            try
            {
                Product product = _productRepository.GetProductBy(slug);
                int output = 0;
                if (product != null && product.Position > 1)
                {
                    List<Product> products = _productRepository.GetProductByPosition(product.Position - 1, product.Position);
                    Product down = products.Where(p => p.Position == product.Position - 1).FirstOrDefault();
                    if (down != null)
                    {
                        product.Position--;
                        down.Position++;
                        // output = _productRepository.Save();
                        Result result = _productRepository.UpdateRange(new Product[] { product, down });
                        output = result.Success ? 1 : 0;
                    }
                    else
                    {
                        product.Position--;
                        Result result = _productRepository.Update(product);
                        output = result.Success ? 1 : 0;
                    }
                }
                return MyResult<bool>.ResultSuccess(output > 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<bool>.ResultError(false, ex);
            }
        }

        public MyResult<ProductResponse> SaveImages(string[] files, string slug, DataUser admin)
        {
            try
            {
                ProductResponse productResponse = null;
                ProductImage[] productImages = new ProductImage[files.Length];
                Product product = _productRepository.GetProductBy(slug);
                int output = 0;
                if (product != null && product.Id > 0)
                {
                    List<ProductImage> oldImages = _productRepository.GetImages(product.Id);
                    List<int> positions = new List<int>();
                    for (int i = 0; i < files.Length + oldImages.Count; i++)
                    {
                        if (!oldImages.Exists(o => o.Position == i))
                        {
                            positions.Add(i);
                        }
                    }
                    bool p0 = oldImages.Exists(o => o.Position == 0);
                    for (int i = 0; i < files.Length; i++)
                    {
                        int n = 0;
                        string filename = files[i];
                        int position = oldImages.Count == 0 ? i : positions[i];
                        while (oldImages.Exists(o => o.Filename == files[i]))
                        {
                            n++;
                            files[i] = $"{filename}_{n}";
                        }
                        productImages[i] = new ProductImage
                        {
                            Id = Guid.NewGuid().ToString("n").Substring(0, 20),
                            //IsMaster = i == 0 && oldImages.Count == 0,
                            IsMaster = (i == 0 && !p0),
                            Filename = files[i],
                            ProductId = product.Id,
                            Position = position
                        };
                    }
                    //oldImages = oldImages.Except(productImages).ToList();
                    //if (oldImages.Count > 0)
                    //{
                    //    int output = _productRepository.DeleteImages(oldImages);
                    //}
                    //oldImages = _productRepository.GetImages(product.Id);
                    //productImages = productImages.Except(oldImages).ToArray();
                    if (productImages.Length > 0)
                    {
                        Result result = _productImageRepository.AddRange(productImages);

                    }
                    string image = p0 ? oldImages.Find(o => o.Position == 0).Filename
                        : productImages[0].Filename;
                    var updateImages = oldImages.Where(o => o.Position > 0 && o.IsMaster).ToList();
                    if (updateImages.Count > 0)
                    {
                        updateImages.ForEach(u => u.IsMaster = false);
                        //output = _productRepository.Save();

                        Result result = _productRepository.Update(product);
                        output = result.Success ? 1 : 0;
                    }
                    if (product.Image != image)
                    {
                        product.Image = image;
                        Result result = _productRepository.Update(product);
                        output = result.Success ? 1 : 0;
                    }

                    productResponse = new ProductResponse
                    {
                        Slug = slug,
                    };
                    return MyResult<ProductResponse>.ResultSuccess(productResponse);
                }
                else
                {
                    return MyResult<ProductResponse>.ResultError(productResponse, _messagesHelper.GetMessageCode(MyResultCode.SlugInvalid), MyResultCode.SlugInvalid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<ProductResponse>.ResultError(null, ex);
            }
        }

        public ImageView[] UpdateImages(string slug, List<ImageView> images, DataUser dataUser)
        {
            try
            {
                Product product = _productRepository.GetProductBy(slug);
                List<ProductImage> pimages = _productRepository.GetImages(product.Id);
                List<ProductImage> deleteImages = new List<ProductImage>();
                //int i = 0;
                //foreach (var item in images)
                //{
                //    ImageView image = images.FindLast(img => img.Position == item.Position);
                //    if (image != item && item.Position == image.Position)
                //    {
                //        image.Position++;
                //    }
                //}
                bool update = false;
                foreach (ProductImage item in pimages)
                {
                    var imageView = images.SingleOrDefault(img => img.ImageGuid == item.Id);
                    if (imageView != null)
                    {
                        if (item.Position != imageView.Position || item.IsMaster != (imageView.Position == 0))
                        {
                            update = true;
                            item.Position = imageView.Position;
                            item.IsMaster = imageView.Position == 0;
                        }
                    }
                    else
                    {
                        deleteImages.Add(item);
                    }
                }
                if (update)
                {
                    Result result = _productImageRepository.UpdateRange(pimages);
                }
                var pimg = pimages.Find(p => p.Position == 0 && !deleteImages.Contains(p));
                if (pimg != null)
                {
                    if (product.Image != pimg.Filename)
                    {
                        product.Image = pimg.Filename;
                        _productRepository.Update(product);
                    }
                }
                if (deleteImages.Count > 0)
                {
                    Result result = _productImageRepository.DeleteRange(deleteImages.ToArray());
                }
                return images.OrderBy(i => i.Position).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public MyResult<ProductResponse> CreateProduct(ProductRequest productRequest, DataUser admin)
        {
            ProductResponse productResponse = null;
            try
            {
                Product product = null;
                if (string.IsNullOrWhiteSpace(productRequest.Name))
                {
                    return MyResult<ProductResponse>.ResultError(productResponse, _messagesHelper.GetMessageCode(MyResultCode.ProductNameRequired), MyResultCode.ProductNameRequired);
                }
                FilterName(productRequest);
                FilterPrice(productRequest);

                Regex reg = new Regex("[^a-zA-Z0-9]");

                string slug = reg.Replace(productRequest.Name.Trim().ToLower(), "_");

                product = _productRepository.GetProductBy(slug);
                if (product != null)
                {
                    return MyResult<ProductResponse>.ResultError(productResponse, _messagesHelper.GetMessageCode(MyResultCode.SlugAlreadyExists), MyResultCode.SlugAlreadyExists);
                }

                Category category = _categoryRepository.GetById(productRequest.CategoryId);
                if (category == null)
                {
                    return MyResult<ProductResponse>.ResultError(productResponse, _messagesHelper.GetMessageCode(MyResultCode.CategoryNotExist), MyResultCode.CategoryNotExist);
                }

                productResponse = new ProductResponse
                {
                    Slug = slug
                };

                product = new Product
                {
                    Id = 0,
                    Admin = admin.Username,
                    Active = productRequest.Active,
                    CategoryId = category?.Id,
                    Slug = slug,
                    Name = productRequest.Name,
                    ShortName = productRequest.ShortName,
                    MetaDescription = productRequest.MetaDescription?.Trim(),
                    MetaKeywords = productRequest.MetaKeywords?.Trim(),
                    MetaTitle = productRequest.MetaTitle?.Trim(),
                    OldPrice = productRequest.OldPrice,
                    NewPrice = productRequest.NewPrice,
                    IsOffer = Calcul.IsOffer(productRequest.OldPrice, productRequest.NewPrice),
                    Offer = Calcul.Offer(productRequest.OldPrice, productRequest.NewPrice),
                    Quantity = productRequest.Quantity,
                    Rating = productRequest.Rating,
                    Description = productRequest.Description?.Trim(),
                    Details = productRequest.Details?.Trim(),
                    Specification = productRequest.Specification?.Trim(),
                    MainCharacteristics = productRequest.MainCharacteristics?.Trim(),
                    TechnicalDescription = productRequest.TechnicalDescription?.Trim(),
                    General = productRequest.General?.Trim(),
                    Garantie = productRequest.Garantie?.Trim(),
                    VenduWith = productRequest.VenduWith?.Trim(),
                };
                Result<Product> result = _productRepository.Add(product);
                int output = result.Success ? 1 : 0;
                if (output > 0)
                {
                    category.CountProducts = _categoryRepository.CategoryCountProducts(category.Id);
                    //output = _categoryRepository.Save();
                    Result<Category> resultUpdate = _categoryRepository.Update(category);
                    output = resultUpdate.Success ? 1 : 0;
                }
                return MyResult<ProductResponse>.ResultSuccess(productResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<ProductResponse>.ResultError(productResponse, ex);
            }
        }

        //public Result<ProductResponse> UpdateProduct(ProductRequest productRequest, DataUser dataUser)
        //{
        //    ProductResponse productResponse = null;
        //    try
        //    {
        //        productRequest.Name = productRequest.Name.Trim().Replace("\"", "");
        //        while (productRequest.Name.Contains("  "))
        //        {
        //            productRequest.Name = productRequest.Name.Replace("  ", " ");
        //        }
        //        Product product = _productRepository.GetProductBy(productRequest.Slug);
        //        productResponse = new ProductResponse
        //        {
        //            Slug = productRequest.Slug
        //        };
        //        int output = 0;
        //        if (productRequest.OldPrice == 0)
        //        {
        //            productRequest.OldPrice = productRequest.NewPrice;
        //        }
        //        if (product != null)
        //        {
        //            //product.Id = oldProduct != null ? oldProduct.Id : 0,
        //            //product.Admin = admin.Username,
        //            product.Active = productRequest.Active;
        //            product.CategoryId = productRequest.CategoryId;
        //            //product.Slug = slug,
        //            product.Name = productRequest.Name;
        //            product.MetaDescription = productRequest.MetaDescription;
        //            product.MetaKeywords = productRequest.MetaKeywords;
        //            product.MetaTitle = productRequest.MetaTitle;
        //            product.OldPrice = productRequest.OldPrice;
        //            product.NewPrice = productRequest.NewPrice;
        //            product.IsOffer = Calcul.IsOffer(productRequest.OldPrice, productRequest.NewPrice);
        //            product.Offer = Calcul.Offer(productRequest.OldPrice, productRequest.NewPrice);
        //            product.Quantity = productRequest.Quantity;
        //            product.Rating = productRequest.Rating;
        //            product.Description = productRequest.Description;
        //            product.Details = productRequest.Details;
        //            product.Specification = productRequest.Specification;
        //            output = _productRepository.Save();
        //            return Result<ProductResponse>.ResultSuccess(productResponse);
        //        }
        //        else
        //        {
        //            return Result<ProductResponse>.ResultError(productResponse, _messagesHelper.GetMessageCode(ResultCode.UpdateProductError), ResultCode.UpdateProductError);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Result<ProductResponse>.ResultError(productResponse, ex);
        //    }
        //}

        public MyResult<int> RemoveProduct(string slug, DataUser dataUser)
        {
            try
            {
                Product product = _productRepository.GetProductBy(slug);
                if (product != null)
                {
                    int output = _productRepository.RemoveProduct(product);
                    if (output > 0 && product.CategoryId.HasValue)
                    {
                        Category category = _categoryRepository.GetById(product.CategoryId.Value);
                        if (category != null)
                        {
                            category.CountProducts = _categoryRepository.CategoryCountProducts(category.Id);
                            _categoryRepository.Save();
                        }
                        return MyResult<int>.ResultSuccess(output);
                    }
                    else
                    {
                        return MyResult<int>.ResultError(default, MyResultCode.RemoveProductError, _messagesHelper.GetMessageCode(MyResultCode.RemoveProductError));
                    }
                }
                else
                {
                    return MyResult<int>.ResultError(default, MyResultCode.RemoveProductError, _messagesHelper.GetMessageCode(MyResultCode.RemoveProductError));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<int>.ResultError(default, MyResultCode.RemoveProductError, _messagesHelper.GetMessageCode(MyResultCode.RemoveProductError));
            }
        }

        public MyResult<int> DesactivateProduct(string slug, DataUser dataUser)
        {
            try
            {
                Product product = _productRepository.GetProductBy(slug);
                if (product != null)
                {
                    product.Active = false;
                    Result<Product> result = _productRepository.Update(product);
                    int output = result.Success ? 1 : 0;
                    if (output > 0 && product.CategoryId.HasValue)
                    {
                        Category category = _categoryRepository.GetById(product.CategoryId.Value);
                        if (category != null)
                        {
                            category.CountProducts = _categoryRepository.CategoryCountProducts(category.Id);
                            _categoryRepository.Save();
                        }
                        return MyResult<int>.ResultSuccess(output);
                    }
                    else
                    {
                        return MyResult<int>.ResultError(default, MyResultCode.RemoveProductError, _messagesHelper.GetMessageCode(MyResultCode.RemoveProductError));
                    }
                }
                else
                {
                    return MyResult<int>.ResultError(default, MyResultCode.RemoveProductError, _messagesHelper.GetMessageCode(MyResultCode.RemoveProductError));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<int>.ResultError(default, MyResultCode.RemoveProductError, _messagesHelper.GetMessageCode(MyResultCode.RemoveProductError));
            }
        }

        public IEnumerable<OrderView> GetOrdersView(OrderFilterAdmin orderFilterAdmin, DataUser admin)
        {
            try
            {
                IEnumerable<OrderView> orders = _orderRepository.GetOrdersView((EnumOrderStatus?)orderFilterAdmin.Status, orderFilterAdmin.Search);
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public OrderView GetOrderView(string orderNumber)
        {
            try
            {
                OrderView order = _orderRepository.GetOrderView(orderNumber);
                if (order != null)
                {
                    IEnumerable<OrderTracking> trackings = _orderRepository.GetOrderTrackings(order.Id);
                    order.ProcessAt = trackings.LastOrDefault(t => t.Status == EnumOrderStatus.Processing)?.Date;
                    order.DeliveredAt = trackings.LastOrDefault(t => t.Status == EnumOrderStatus.Completed)?.Date;
                }
                //if (order?.Items != null)
                //{
                //    foreach (OrderItemView orderItemView in order.Items)
                //    {
                //        if (orderItemView.Image != null)
                //        {
                //            orderItemView.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), orderItemView.Slug, orderItemView.Image);
                //        }
                //    }
                //}
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public OrderItemView GetOrderDetailsView(string orderItemNumber)
        {
            try
            {
                OrderItemView orderItemView = _orderRepository.GetOrderDetailsView(orderItemNumber);
                return orderItemView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public MyResult<int> UpdateOrderStatus(string orderNumber, EnumOrderStatus status, DataUser dataUser)
        {
            try
            {
                Order order = _orderRepository.GetOrderBy(orderNumber);
                int output = 0;
                bool valid = EnumExtension.EnumValid(typeof(EnumOrderStatus), (int)status);
                if (!valid)
                {
                    return MyResult<int>.ResultError(output, _messagesHelper.GetMessageCode(MyResultCode.StatusOrderInvalid), MyResultCode.StatusOrderInvalid);
                }
                if (valid && order != null && order.Status != status)
                {
                    order.Status = status;
                    if (status == EnumOrderStatus.Completed)
                    {
                        order.IsPaid = true;
                    }
                    if (status == EnumOrderStatus.Canceled)
                    {
                        order.IsPaid = false;
                    }
                    if (status == EnumOrderStatus.Processing)
                    {
                        order.IsPaid = false;
                    }

                    Result<Order> result = _orderRepository.Update(order);
                    OrderTracking orderTracking = new OrderTracking
                    {
                        OrderId = order.Id,
                        Status = status,
                        Date = DateTime.UtcNow
                    };
                    _orderTrackingRepository.Add(orderTracking);
                }
                return MyResult<int>.ResultSuccess(output);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<int>.ResultError(default, ex);
            }
        }

        public IEnumerable<OrderTracking> GetOrderTrackings(string orderNumber, DataUser dataUser)
        {
            try
            {
                Order order = _orderRepository.GetOrderBy(orderNumber);
                if (order != null)
                {
                    IEnumerable<OrderTracking> orderTracking = _orderRepository.GetOrderTrackings(orderId: order.Id);
                    return orderTracking;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }

        public MyResult<int> RemoveImage(string slug, string imageGuid, DataUser dataUser)
        {
            //ProductImage productImage = _productImageRepository.GetById(imageGuid);
            //if (productImage != null)
            //{
            //    Product product = _productRepository.GetById(productImage.ProductId);
            //    ProductImage[] images = _productImageRepository.GetAll((req) => req.AddPredicate((p) => p.ProductId == productImage.ProductId));
            //    Result resultDelete = _productImageRepository.Delete(productImage);
            //    //(string filename, string slug) = _productRepository.RemoveProductImage(imageGuid);
            //    if (!string.IsNullOrWhiteSpace(productImage.Filename) && !string.IsNullOrWhiteSpace(product.Slug))
            //    {
            //        string directory = ApplicationSettingsAccessor.GetDirectoryImages();
            //        string directoryImage = HelperFile.FullPathDirectoryImage(directory, product.Slug);
            //        string fullPath = HelperFile.FullPathImage(directoryImage, productImage.Filename);
            //        if (System.IO.File.Exists(fullPath))
            //        {
            //            System.IO.File.Delete(fullPath);
            //        }
            //        return MyResult<int>.ResultSuccess(1);
            //    }
            //    else
            //    {
            //        return MyResult<int>.ResultError(default, MyResultCode.RemoveProductImageError, _messagesHelper.GetMessageCode(MyResultCode.RemoveProductImageError));
            //    }
            //}
            //else
            //{
            //    return MyResult<int>.ResultError(default, MyResultCode.RemoveProductImageError, _messagesHelper.GetMessageCode(MyResultCode.RemoveProductImageError));
            //}
            return MyResult<int>.ResultSuccess(0);
        }

        public ProductEdit GetProductEdit(string slug, DataUser dataUser)
        {
            try
            {
                ProductEdit product = _productRepository.GetProductEdit(slug);
                if (product != null && product.Images != null)
                {
                    //int i = 0;
                    product.Images = product.Images.OrderBy(img => img.Position).ToArray();
                    //foreach (ImageView item in product.Images)
                    //{
                    //    item.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), product.Slug, item.Image);
                    //    if (item.Position != i)
                    //    {
                    //        item.Position = i;
                    //    }
                    //    i++;
                    //}
                }
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public MyResult<ProductResponse> UpdateProduct(ProductRequest productRequest, DataUser dataUser)
        {
            ProductResponse productResponse = null;
            try
            {
                if (string.IsNullOrWhiteSpace(productRequest.Name))
                {
                    return MyResult<ProductResponse>.ResultError(productResponse, _messagesHelper.GetMessageCode(MyResultCode.ProductNameRequired), MyResultCode.ProductNameRequired);
                }
                FilterName(productRequest);
                FilterPrice(productRequest);

                Product product = _productRepository.GetProductBy(productRequest.Slug);

                productResponse = new ProductResponse
                {
                    Slug = productRequest.Slug
                };
                int output = 0;

                Category category = _categoryRepository.GetById(productRequest.CategoryId);
                if (category == null)
                {
                    return MyResult<ProductResponse>.ResultError(productResponse, _messagesHelper.GetMessageCode(MyResultCode.CategoryObligatory), MyResultCode.CategoryObligatory);
                }

                if (product != null)
                {
                    Category oldCategory = null;
                    if (product.CategoryId.HasValue)
                    {
                        oldCategory = _categoryRepository.GetById(product.CategoryId.Value);
                    }

                    //product.Id = oldProduct != null ? oldProduct.Id : 0,
                    //product.Admin = admin.Username,
                    product.Active = productRequest.Active;
                    product.CategoryId = productRequest.CategoryId;
                    //product.Slug = slug,
                    product.Name = productRequest.Name;
                    product.ShortName = productRequest.ShortName ?? "";
                    product.MetaDescription = productRequest.MetaDescription?.Trim();
                    product.MetaKeywords = productRequest.MetaKeywords?.Trim();
                    product.MetaTitle = productRequest.MetaTitle?.Trim();
                    product.OldPrice = productRequest.OldPrice;
                    product.NewPrice = productRequest.NewPrice;
                    product.IsOffer = Calcul.IsOffer(productRequest.OldPrice, productRequest.NewPrice);
                    product.Offer = Calcul.Offer(productRequest.OldPrice, productRequest.NewPrice);
                    product.Quantity = productRequest.Quantity;
                    product.Rating = productRequest.Rating;
                    product.Description = productRequest.Description?.Trim();
                    product.Details = productRequest.Details?.Trim();
                    product.Specification = productRequest.Specification?.Trim();
                    product.MainCharacteristics = productRequest.MainCharacteristics?.Trim();
                    product.TechnicalDescription = productRequest.TechnicalDescription?.Trim();
                    product.General = productRequest.General?.Trim();
                    product.Garantie = productRequest.Garantie?.Trim();
                    product.VenduWith = productRequest.VenduWith?.Trim();

                    //output = _productRepository.Save();

                    Result<Product> result = _productRepository.Update(product);
                    output = result.Success ? 1 : 0;

                    if (oldCategory != null)
                    {
                        oldCategory.CountProducts = _categoryRepository.CategoryCountProducts(oldCategory.Id);
                        if (oldCategory.Id != category.Id)
                        {
                            category.CountProducts = _categoryRepository.CategoryCountProducts(category.Id);
                        }
                        output = _categoryRepository.Save();

                        // count all products

                        List<Category> categories = _categoryRepository.GetAllCategories();
                        CountAllProductsCategory(categories, category);
                    }
                    else
                    {
                        category.CountProducts = _categoryRepository.CategoryCountProducts(category.Id);
                        _categoryRepository.Save();

                        // count all products

                        List<Category> categories = _categoryRepository.GetAllCategories();
                        CountAllProductsCategory(categories, category);
                    }
                    return MyResult<ProductResponse>.ResultSuccess(productResponse);
                }
                else
                {
                    return MyResult<ProductResponse>.ResultError(productResponse, _messagesHelper.GetMessageCode(MyResultCode.UpdateProductError), MyResultCode.UpdateProductError);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<ProductResponse>.ResultError(productResponse, ex);
            }
        }

        private void CountAllProductsCategory(List<Category> categories, Category category)
        {
            try
            {
                List<Category> children = _categoryRepository.GetChildrenCategory(category.Id, false, false);
                int output = 0;
                if (children.Count > 0)
                {
                    Action<Category, Category[], int> handler =
                        (cat, cats, level) =>
                        {
                            if (cats != null)
                            {
                                cat.CountAllProducts = cats.Sum(c => c.CountAllProducts) + cat.CountProducts;
                            }
                            else
                            {
                                cat.CountAllProducts = cat.CountProducts;
                            }
                        };

                    _categoryRepository.ForChildren(children.ToArray(), categories, false, 1, handler);

                    category.CountAllProducts = children.Sum(c => c.CountAllProducts) + category.CountProducts;

                    output = _categoryRepository.Save();
                }
                else
                {
                    category.CountAllProducts = category.CountProducts;
                    output = _categoryRepository.Save();
                }

                while (category.ParentId.HasValue)
                {
                    category = _categoryRepository.GetById(category.ParentId.Value);
                    children = categories.Where(c => c.ParentId == category.Id).ToList();

                    category.CountAllProducts = children.Sum(c => c.CountAllProducts) + category.CountProducts;
                }
                output = _categoryRepository.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private void FilterPrice(ProductRequest productRequest)
        {
            if (productRequest.OldPrice == 0)
            {
                productRequest.OldPrice = productRequest.NewPrice;
            }
            if (productRequest.NewPrice == 0)
            {
                productRequest.NewPrice = productRequest.OldPrice;
            }
        }

        private void FilterName(ProductRequest productRequest)
        {
            productRequest.Name = productRequest.Name.Trim().Replace("\"", "");
            while (productRequest.Name.Contains("  "))
            {
                productRequest.Name = productRequest.Name.Replace("  ", " ");
            }
        }

        public MyResult<int> UpdateAllSearchTerms(DataUser dataUser)
        {
            try
            {
                Result result = _productRepository.UpdateRange(_productRepository.GetAll());
                int output = result.Success ? 1 : 0;
                return MyResult<int>.ResultSuccess(output);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return MyResult<int>.ResultError(0, ex);
            }
        }

        public void UpdateImagesPosition(DataUser dataUser)
        {
            try
            {
                var images = _productImageRepository.GetAll();
                Dictionary<int, ProductImage[]> imagesToUpdate = new Dictionary<int, ProductImage[]>();
                foreach (var item in images.GroupBy((img) => img.ProductId))
                {
                    if (item.Count(m => m.IsMaster) > 0)
                    {
                        imagesToUpdate.Add(item.Key, item.ToArray());
                    }
                    else if (item.GroupBy(m => m.Position).Count() > 1)
                    {
                        imagesToUpdate.Add(item.Key, item.ToArray());
                    }
                }
                foreach (var item in imagesToUpdate)
                {
                    if (item.Value.Count(m => m.IsMaster) > 1)
                    {
                        ProductImage[] imagesUpdate = item.Value.OrderBy(m => m.Position).ToArray();
                        var pimg = imagesUpdate.FirstOrDefault(m => m.Position == 0);
                        if (pimg == null)
                        {
                            pimg = imagesUpdate.First();
                        }
                        pimg.IsMaster = true;
                        pimg.Position = 0;
                        int i = 1;
                        imagesUpdate.Where(m => m != pimg).ToList().ForEach(m => { m.IsMaster = false; m.Position = i++; });
                        Result result = _productImageRepository.UpdateRange(imagesUpdate);
                    }
                    else if (item.Value.GroupBy(m => m.Position).Count() < item.Value.Length)
                    {
                        ProductImage[] imagesUpdate = item.Value.OrderBy(m => m.Position).ToArray();
                        var pimg = imagesUpdate.FirstOrDefault(m => m.IsMaster && m.Position == 0);
                        if (pimg == null)
                        {
                            pimg = imagesUpdate.FirstOrDefault(m => m.IsMaster);
                            if (pimg == null)
                            {
                                pimg = imagesUpdate.FirstOrDefault(m => m.Position == 0);
                                if (pimg == null)
                                {
                                    pimg = imagesUpdate.First();
                                }
                            }
                        }
                        pimg.IsMaster = true;
                        pimg.Position = 0;
                        int i = 1;
                        imagesUpdate.Where(m => m != pimg).ToList().ForEach(m => { m.IsMaster = false; m.Position = i++; });
                        Result result = _productImageRepository.UpdateRange(imagesUpdate);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        public MyResult<Admin[]> GetSellers(DataUser dataUser)
        {
            var admins = _adminRepository.GetAll();
            return MyResult<Admin[]>.ResultSuccess(admins);
        }
        public MyResult<string> CreateSeller(AdminPutModel model, DataUser dataUser)
        {
            var all = _adminRepository.GetAll(req => req.AddPredicate(a => a.Email == model.Email || a.Username == model.Username));
            if (all.Length > 0)
            {
                return MyResult<string>.ResultError("", _messagesHelper.GetMessageCode(MyResultCode.CreateSellerExiste), MyResultCode.CreateSellerExiste);
            }
            Admin admin = new Admin
            {
                Id = 0,
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                Email = model.Email,
                Username = model.Username,
                Status = EnumStatusAccount.Active
            };
            admin.Role = EnumRole.Admin;
            admin.RegistrationDate = DateTime.UtcNow;

            HashMD5 hashMD5 = new HashMD5();
            if (!string.IsNullOrEmpty(model.Password))
            {
                admin.PasswordHash = hashMD5.GetMd5Hash(model.Password);
            }

            common.Result output = _adminRepository.Add(admin);
            return MyResult<string>.ResultSuccess("");
        }

    }
}
