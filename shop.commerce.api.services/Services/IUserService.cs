using shop.commerce.api.Application.Configuration;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.services.Models;
using System.Collections.Generic;

namespace shop.commerce.api.services.Services
{
    public interface IUserService
    {
        IApplicationSettingsAccessor ApplicationSettingsAccessor { get; set; }

        List<ProductView> GetProductsView(ProductFilterUser productFilter, DataUser seller);
        ProductDetailView GetProductDetailView(string slug, DataUser seller);
        MyResult<OrderView> CreateOrder(OrderRequest orderRequest, DataUser dataUser);
        List<SelectView> GetPaymentMethods(DataUser dataUser);
        Order GetOrderBy(string orderNumber);
        OrderView GetOrderViewBy(string orderNumber);
        List<SelectView> GetOrderStatus(DataUser dataUser);
        List<ProductView> GetRelatedProductsBySlug(string slug, int count, DataUser dataUser);
        List<ProductView> GetRelatedProductsByCategory(int categoryId, int count, DataUser dataUser);
        ResultPage<ProductView> GetProductsViewPage(ProductFilterUser productFilter, DataUser dataUser);
    }
}
