using shop.commerce.api.Application.Configuration;
using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Request;
using shop.commerce.api.domain.Models.Response;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.domain.Entities;
using shop.commerce.api.services.Models;
using System.Collections.Generic;

namespace shop.commerce.api.services.Services
{
    public interface IAdminService
    {
        IApplicationSettingsAccessor ApplicationSettingsAccessor { get; set; }

        MyResult<ProductResponse> CreateProduct(ProductRequest product, DataUser seller);
        MyResult<ProductResponse> UpdateProduct(ProductRequest product, DataUser dataUser);
        MyResult<ProductResponse> SaveImages(string[] files, string slug, DataUser seller);
        List<ProductView> GetProductsView(ProductFilterAdmin productFilter, DataUser seller);
        ResultPage<ProductView> GetProductsViewPage(ProductFilterAdmin productFilter, DataUser seller);
        ProductDetailView GetProductDetailView(string slug, DataUser seller);
        IEnumerable<OrderView> GetOrdersView(OrderFilterAdmin orderFilterAdmin, DataUser dataUser);
        MyResult<int> UpdateOrderStatus(string orderNumber, EnumOrderStatus status, DataUser dataUser);
        IEnumerable<OrderTracking> GetOrderTrackings(string orderNumber, DataUser dataUser);
        OrderItemView GetOrderDetailsView(string orderItemNumber);
        MyResult<int> RemoveProduct(string slug, DataUser dataUser);
        MyResult<int> RemoveImage(string slug, string imageGuid, DataUser dataUser);
        ProductEdit GetProductEdit(string slug, DataUser dataUser);
        OrderView GetOrderView(string orderNumber);
        ImageView[] UpdateImages(string slug, List<ImageView> images, DataUser dataUser);
        MyResult<bool> OrderProducts(bool force);
        MyResult<bool> UpProduct(string slug);
        MyResult<bool> DownProduct(string slug);
        MyResult<int> UpdateAllSearchTerms(DataUser dataUser);
        void UpdateImagesPosition(DataUser dataUser);
        MyResult<int> DesactivateProduct(string slug, DataUser dataUser);
        MyResult<string> CreateSeller(AdminPutModel model, DataUser dataUser);
        MyResult<Admin[]> GetSellers(DataUser dataUser);
    }
}
