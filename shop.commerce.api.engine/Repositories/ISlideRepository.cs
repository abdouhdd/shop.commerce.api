using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories.Entities;
using System.Collections.Generic;

namespace shop.commerce.api.infrastructure.Repositories
{
    public interface ISlideRepository : IRepository<Slide, int>
    {
        SlideView GetLastSlideView();
        Slide GetSlide(int slideId);
        List<Slide> GetSlides();
        List<SlideView> GetSlidesView();
        int InsertSlide(Slide slide);
        int RemovetSlide(int slideId);
        int UpdateSlide(Slide slide);
    }
}
