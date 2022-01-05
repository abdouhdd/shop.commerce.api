using shop.commerce.api.Application.Configuration;
using shop.commerce.api.common;
using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Extensions;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.services.Helpers;
using shop.commerce.api.services.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.services.Services
{
    public class UserService : IUserService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly MessagesHelper _messagesHelper;
        public IApplicationSettingsAccessor ApplicationSettingsAccessor { get; set; }

        public UserService(IProductRepository productRepository, ICategoryRepository categoryRepository, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, MessagesHelper messagesHelper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _messagesHelper = messagesHelper;
        }

        public ProductDetailView GetProductDetailView(string slug, DataUser seller)
        {
            ProductDetailView productDetailView = _productRepository.GetProductDetailView(slug);
            if (productDetailView != null)
            {
                var categories = _categoryRepository.GetAllCategories();
                var categoriesModel = new List<CategoryModelView>();
                Category category = categories.SingleOrDefault(c => c.Id == productDetailView.CategoryId);
                if (category != null)
                {
                    categoriesModel.Add(new CategoryModelView { Id = category.Id, Name = category.Name });
                    while (category.ParentId > 0)
                    {
                        category = categories.SingleOrDefault(c => c.Id == category.ParentId);
                        if (category != null)
                        {
                            categoriesModel.Add(new CategoryModelView { Id = category.Id, Name = category.Name }); 
                        }
                    }
                }
                categoriesModel.Reverse();
                productDetailView.Categories = categoriesModel.ToArray();
                //for (int i = 0; i < productDetailView.Images.Length; i++)
                //{
                //    productDetailView.Images[i] = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), slug, productDetailView.Images[i]);
                //}
                //productDetailView.Categories = GetPathCategories(productDetailView.CategoryId);
                Result<Product> result = _productRepository.Update(productDetailView.Id, (p) =>
                        {
                            p.CountView += 1;
                        });
            }
            return productDetailView;
        }

        private CategoryView[] GetPathCategories(int? categoryId)
        {
            return null;
        }

        public List<ProductView> GetRelatedProductsBySlug(string slug, int count, DataUser dataUser)
        {
            Product product = _productRepository.GetProductBy(slug);
            List<ProductView> relatedProducts = null;
            if (product != null && product.CategoryId.HasValue)
            {
                relatedProducts = GetRelatedProductsByCategory(product.CategoryId.Value, count, dataUser);
                if (relatedProducts != null)
                {
                    relatedProducts.RemoveAll(p => p.Slug == slug);
                }
            }
            return relatedProducts;
        }

        public List<ProductView> GetRelatedProductsByCategory(int categoryId, int count, DataUser dataUser)
        {
            List<ProductView> products = _productRepository.GetRelatedProducts(categoryId, count);
            //if (products != null)
            //{
            //    foreach (ProductView product in products)
            //    {
            //        product.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), product.Slug, product.Image);
            //    }
            //}
            return products;
        }

        public List<ProductView> GetProductsView(ProductFilterUser productFilter, DataUser seller)
        {
            List<ProductView> productViews = _productRepository.GetProductsViewUser(productFilter);
            //if (productViews != null)
            //{
            //    foreach (ProductView productView in productViews)
            //    {
            //        productView.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), productView.Slug, productView.Image);
            //    }
            //}
            return productViews;
        }

        public ResultPage<ProductView> GetProductsViewPage(ProductFilterUser productFilter, DataUser dataUser)
        {
            if (productFilter.Categories?.Length > 0)
            {
                productFilter.Categories = _categoryRepository.GetAllChildrenCategories(productFilter.Categories, true);
            }
            else
            {
                productFilter.Categories = null;
            }
            ResultPage<ProductView> resultPage = _productRepository.GetProductsViewUserPage(productFilter);
            return resultPage;
        }

        public MyResult<OrderView> CreateOrder(OrderRequest orderRequest, DataUser dataUser)
        {
            string[] slugs = orderRequest.Items.Select(p => p.Slug).Distinct().ToArray();
            Product[] products = _productRepository.GetProductsBySlugs(slugs);

            Order order = new Order
            {
                Id = 0,
                OrderNumber = Guid.NewGuid().ToString("n").Substring(0, 10),
                Email = orderRequest.Email,
                FullName = orderRequest.FullName,
                Phone = orderRequest.Phone,
                //Username = dataUser.Username,
                Country = orderRequest.Country,
                City = orderRequest.City,
                ZipCode = orderRequest.ZipCode,
                Status = orderRequest.OrderStatus ?? EnumOrderStatus.Pending,
                Address = orderRequest.Address,
                TotalAmount = orderRequest.TotalAmount,
                TotalQty = orderRequest.TotalQty,
                IsPaid = false,
                OrdersNote = orderRequest.OrdersNote,
                PaymentMethod = orderRequest.PaymentMethod,
                AddressIp = orderRequest.AddressIp,
                Browser = orderRequest.Browser,
            };

            OrderItem[] orderItems = new OrderItem[products.Length];
            for (int i = 0; i < products.Length; i++)
            {
                Product product = products[i];
                //OrderItemRequest orderItem = orderRequest.Items[i];
                OrderItemRequest orderItem = orderRequest.Items.Single(oi => oi.Slug == product.Slug);
                orderItems[i] = new OrderItem
                {
                    ProductId = product.Id,
                    Qty = orderItem.Qty,
                    Price = product.NewPrice,
                    TotalPrice = orderItem.Price,
                };
            }

            Result<Order> result = _orderRepository.Add(order);
            foreach (OrderItem orderItem in orderItems)
            {
                orderItem.OrderId = order.Id;
                orderItem.OrderItemNumber = Guid.NewGuid().ToString("n").Substring(0, 10);
            }

            Result resultItems = _orderItemRepository.AddRange(orderItems);

            //foreach (Product product in products)
            //{
            //    OrderItemRequest orderItem = orderRequest.Items.Single(oi => oi.Slug == product.Slug);
            //    product.Quantity -= orderItem.Qty;
            //}
            //output = _orderRepository.Save();

            OrderView orderView = GetOrderViewBy(order.OrderNumber);
            if (orderView != null)
            {
                return MyResult<OrderView>.ResultSuccess(orderView);
            }
            else
            {
                return MyResult<OrderView>.ResultError(orderView, _messagesHelper.GetMessageCode(MyResultCode.OrdreCreateError), MyResultCode.OrdreCreateError);
            }
        }

        public Order GetOrderBy(string orderNumber)
        {
            Order order = _orderRepository.GetOrderBy(orderNumber);
            return order;
        }

        public OrderView GetOrderViewBy(string orderNumber)
        {
            OrderView orderView = _orderRepository.GetOrderViewBy(orderNumber);
            return orderView;
        }

        public List<SelectView> GetPaymentMethods(DataUser dataUser)
        {
            //List<PaymentMethodView> paymentMethods = new List<PaymentMethodView>();

            //Array valuesPaymentMethod = Enum.GetValues(typeof(EnumPaymentMethod));
            //foreach (int value in valuesPaymentMethod)
            //{
            //    var enumType = typeof(EnumPaymentMethod);
            //    var memberInfos = enumType.GetMember(Enum.GetName(typeof(EnumPaymentMethod), value));
            //    var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            //    var valueAttributes =
            //          enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            //    var description = ((DescriptionAttribute)valueAttributes[0]).Description;

            //    paymentMethods.Add(new PaymentMethodView
            //    {
            //        Id = value,
            //        Name = description//Enum.GetName(typeof(EnumPaymentMethod), value)
            //    });
            //}
            //return paymentMethods;

            List<SelectView> paymentMethods = EnumExtension.ValuesEnum(typeof(EnumPaymentMethod));
            return paymentMethods;
        }

        public List<SelectView> GetOrderStatus(DataUser dataUser)
        {
            //List<PaymentMethodView> paymentMethods = new List<PaymentMethodView>();

            //Array valuesPaymentMethod = Enum.GetValues(typeof(EnumPaymentMethod));
            //foreach (int value in valuesPaymentMethod)
            //{
            //    var enumType = typeof(EnumPaymentMethod);
            //    var memberInfos = enumType.GetMember(Enum.GetName(typeof(EnumPaymentMethod), value));
            //    var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            //    var valueAttributes =
            //          enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            //    var description = ((DescriptionAttribute)valueAttributes[0]).Description;

            //    paymentMethods.Add(new PaymentMethodView
            //    {
            //        Id = value,
            //        Name = description//Enum.GetName(typeof(EnumPaymentMethod), value)
            //    });
            //}
            //return paymentMethods;

            List<SelectView> OrderStatus = EnumExtension.ValuesEnum(typeof(EnumOrderStatus));
            return OrderStatus;
        }
    }
}
