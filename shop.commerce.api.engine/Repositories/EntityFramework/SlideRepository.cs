using Microsoft.Extensions.Logging;
using shop.commerce.api.domain.Views;
using shop.commerce.api.domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class SlideRepository : ISlideRepository
    {
        
        public List<SlideView> GetSlidesView()
        {
            return _entity.OrderBy(s=>s.Index).Select(s => new SlideView
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Link = s.Link,
                Image = s.Image,
                Index = s.Index,
                Active = s.Active
            }).ToList();
        }
        
        public List<Slide> GetSlides()
        {
            return _entity.OrderBy(s=>s.Index).ToList();
        }

        public SlideView GetLastSlideView()
        {
            Slide slide = _entity.OrderBy(s => s.Index).LastOrDefault();
            SlideView slideView = null;
            if (slide != null)
            {
                slideView = new SlideView
                {
                    Id = slide.Id,
                    Title = slide.Title,
                    Description = slide.Description,
                    Link = slide.Link,
                    Image = slide.Image,
                    Index = slide.Index,
                    Active = slide.Active
                }; 
            }
            return slideView;
        }

        public int InsertSlide(Slide slide)
        {
            _entity.Add(slide);
            int output = _context.SaveChanges();
            return output;
        }
        
        public int UpdateSlide(Slide slide)
        {
            _context.Update(slide);
            int output = _context.SaveChanges();
            return output;
        }

        public Slide GetSlide(int slideId)
        {
            var slide = _entity.Where(s => s.Id == slideId).SingleOrDefault();
            return slide;
        }

        public int RemovetSlide(int slideId)
        {
            Slide slide = GetSlide(slideId);
            int output = 0;
            if (slide != null)
            {
                _entity.Remove(slide);
                output = _context.SaveChanges();
            }
            return output;
        }
    }

    public partial class SlideRepository : Repository<Slide, int>
    {
        //private readonly ShopContext _context;

        //public SlideRepository(ShopContextFactory shopContextFactory)
        //{
        //    _context = shopContextFactory.GetShopContext();
        //}

        public SlideRepository(ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory)
        {
        }
    }
}
