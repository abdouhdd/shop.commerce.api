using shop.commerce.api.Application.Configuration;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.services.Models;
using System.Collections.Generic;

namespace shop.commerce.api.services.Services
{
    public interface IHomeService
    {
        IApplicationSettingsAccessor ApplicationSettingsAccessor { get; set; }

        SlideView GetLastSlideView(DataUser dataUser);
        List<ProductView> GetLatestProductsViewUser(int count, DataUser dataUser);
        List<ProductView> GetProductsOffer(DataUser dataUser, int count);
        List<ProductView> GetProductsTopRating(DataUser dataUser, int count);
        List<ProductView> GetProductsTopSale(DataUser dataUser, int count);
        Slide GetSlide(int slideId, DataUser dataUser);
        List<SlideView> GetSlideData(bool all, bool active, DataUser dataUser);
        MyResult<SlideView> InsertSlide(SlideView slideView, DataUser dataUser);
        MyResult<int> RemoveSlide(int slideId, DataUser dataUser);
        int SaveSlideImage(byte[] bytes, int slideId, string extension, DataUser dataUser);
        MyResult<SlideView> UpdateSlide(SlideView slide, DataUser dataUser);
        MyResult<int> UpSlide(int slideId, DataUser dataUser);
        MyResult<int> DownSlide(int slideId, DataUser dataUser);
    }
}