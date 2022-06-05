using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.domain.Entities;
using System;
using System.Collections.Generic;

namespace shop.commerce.api.infrastructure.Repositories
{
    public interface IProductRepository : IRepository<Product, int>
    {
        List<ProductView> GetProductsViewAdmin(ProductFilterAdmin productFilter);
        ResultPage<ProductView> GetProductsViewAdminPage(ProductFilterAdmin productFilter, int[] categories);
        List<ProductView> GetProductsViewUser(ProductFilterUser productFilter);
        ResultPage<ProductView> GetProductsViewUserPage(ProductFilterUser productFilter);
        Product GetProductBy(string slug);
        List<Product> FindProductsByIds(int[] ids);
        ProductDetailView GetProductDetailView(string slug);
        //int SaveImages(List<ProductImage> productImages);
        Product[] GetProductsBySlugs(string[] slugs);
        List<ProductImage> GetImages(int productId);
        List<ProductImage> GetImagesBySlug(string slug);
        int DeleteImages(List<ProductImage> productImages);
        //int UpdateProductCountView(int productId);
        List<ProductView> GetLatestProductsViewUser(int count);
        List<ProductView> GetProductsTopRating(int count);
        List<ProductView> GetRelatedProducts(int categoryId, int count);
        List<ProductView> GetProductsOffer(int count);
        List<ProductView> GetProductsTopSale(int count);
        int RemoveProduct(string slug);
        (string filename, string slug) RemoveProductImage(string imageGuid);
        ProductEdit GetProductEdit(string slug);
        List<Product> FindAllProducts();
        bool AllProducts(Func<Product, bool> predicate);
        List<Product> GetProductByPosition(int from, int to = -1);
        int RemoveProduct(Product product);
    }
}
