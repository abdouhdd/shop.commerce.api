using shop.commerce.api.Application.Configuration;
using shop.commerce.api.common;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Views;
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.domain.Entities;
using shop.commerce.api.services.Helpers;
using shop.commerce.api.services.Models;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.services.Services
{
    public class HomeService : IHomeService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ISlideRepository _slideRepository;
        private readonly MessagesHelper _messagesHelper;
        public IApplicationSettingsAccessor ApplicationSettingsAccessor { get; set; }

        public HomeService(IProductRepository productRepository, IOrderRepository orderRepository, ISlideRepository slideRepository, MessagesHelper messagesHelper)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _slideRepository = slideRepository;
            _messagesHelper = messagesHelper;
        }

        public List<SlideView> GetSlideData(bool all, bool active, DataUser dataUser)
        {
            List<Slide> result = _slideRepository.GetSlides();

            if (result.Count > 0)
            {
                int max = 0;
                foreach (Slide item in result.Where(s => s.Index != 0))
                {
                    if (++max != item.Index)
                    {
                        item.Index = max;
                    }
                    int o = _slideRepository.UpdateSlide(item);
                }
                List<Slide> result0 = result.Where(s => s.Index == 0).ToList();
                if (result0.Count > 0)
                {
                    foreach (Slide item in result0)
                    {
                        item.Index = ++max;
                        int o = _slideRepository.UpdateSlide(item);
                    }
                }

                result = result.OrderBy(s => s.Index).ToList();
            }
            List<SlideView> resultView = result.Where(s => all || s.Active == active).Select(s => new SlideView
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Link = s.Link,
                Index = s.Index,
                Image = s.Image,
                Active = s.Active
            }).ToList();
            return resultView;
        }

        public MyResult<SlideView> InsertSlide(SlideView slideView, DataUser dataUser)
        {
            Slide slide = new Slide
            {
                Title = slideView.Title,
                Description = slideView.Description,
                Image = slideView.Image,
                Link = slideView.Link,
                Index = slideView.Index,
                Active = slideView.Active
            };
            int output = _slideRepository.InsertSlide(slide);
            slideView.Id = slide.Id;
            if (output > 0)
            {
                return MyResult<SlideView>.ResultSuccess(slideView);
            }
            else
            {
                return MyResult<SlideView>.ResultError(slideView, _messagesHelper.GetMessageCode(MyResultCode.InsertSlideError), MyResultCode.InsertSlideError);
            }
        }

        public SlideView GetLastSlideView(DataUser dataUser)
        {
            SlideView slideView = _slideRepository.GetLastSlideView();
            return slideView;
        }

        public MyResult<SlideView> UpdateSlide(SlideView slideView, DataUser dataUser)
        {
            int output = 0;
            Slide slideTrouve = _slideRepository.GetSlide(slideView.Id);
            if (slideTrouve != null)
            {
                slideTrouve.Title = slideView.Title;
                slideTrouve.Description = slideView.Description;
                if (!string.IsNullOrEmpty(slideView.Image))
                {
                    slideTrouve.Image = slideView.Image; 
                }
                slideTrouve.Link = slideView.Link;
                slideTrouve.Active = slideView.Active;
                //slideTrouve.Index = slideView.Index;
                Result<Slide> result = _slideRepository.Update(slideTrouve);
                output = result.Success ? 1 : 0;
            }
            return MyResult<SlideView>.ResultSuccess(slideView);
            //if (output > 0)
            //{
            //}
            //else
            //{
            //    return Result<SlideView>.ResultError(slideView, _messagesHelper.GetMessageCode(ResultCode.UpdateSlideError), ResultCode.InsertSlideError);
            //}
        }

        public MyResult<int> RemoveSlide(int slideId, DataUser dataUser)
        {

            int output = _slideRepository.RemovetSlide(slideId);
            if (output > 0)
            {
                return MyResult<int>.ResultSuccess(output);
            }
            else
            {
                return MyResult<int>.ResultError(output, _messagesHelper.GetMessageCode(MyResultCode.RemoveSlideError), MyResultCode.RemoveSlideError);
            }
        }

        public Slide GetSlide(int slideId, DataUser dataUser)
        {
            Slide slide = _slideRepository.GetSlide(slideId);
            return slide;
        }

        public MyResult<int> UpSlide(int slideId, DataUser dataUser)
        {
            List<Slide> result = _slideRepository.GetSlides();
            int output = 0;
            if (result?.Count > 0)
            {
                // result = result.OrderBy(s => s.Index).ToList();
                var slide = result.FirstOrDefault(s => s.Id == slideId);
                if (slide != null)
                {
                    var slideUp = result.FirstOrDefault(s => s.Index == slide.Index+1);
                    if (slideUp != null)
                    {
                        slide.Index++;
                        slideUp.Index--;
                        output = _slideRepository.UpdateSlide(slide);
                        output = _slideRepository.UpdateSlide(slideUp);
                    } 
                }
            }

            if (output > 0)
            {
                return MyResult<int>.ResultSuccess(output);
            }
            else
            {
                return MyResult<int>.ResultError(output, _messagesHelper.GetMessageCode(MyResultCode.UpSlideError), MyResultCode.UpSlideError);
            }
        }

        public MyResult<int> DownSlide(int slideId, DataUser dataUser)
        {
            List<Slide> result = _slideRepository.GetSlides();
            int output = 0;
            if (result?.Count > 0)
            {
                //result = result.OrderBy(s => s.Index).ToList();
                var slide = result.FirstOrDefault(s => s.Id == slideId);
                if (slide != null && slide.Index > 1)
                {
                    slide.Index--;
                    var slideDown = result.FirstOrDefault(s => s.Index == slide.Index);
                    output = _slideRepository.UpdateSlide(slide);
                    if (output > 0 && slideDown != null)
                    {
                        slideDown.Index++;
                        _slideRepository.UpdateSlide(slideDown);
                    }
                }
            }

            if (output > 0)
            {
                return MyResult<int>.ResultSuccess(output);
            }
            else
            {
                return MyResult<int>.ResultError(output, _messagesHelper.GetMessageCode(MyResultCode.DownSlideError), MyResultCode.DownSlideError);
            }
        }

        public List<ProductView> GetProductsTopRating(DataUser dataUser, int count)
        {
            List<ProductView> topProductsRating = _productRepository.GetProductsTopRating(count);
            //if (topProductsRating != null)
            //{
            //    foreach (ProductView productView in topProductsRating)
            //    {
            //        productView.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), productView.Slug, productView.Image);
            //    }
            //}
            return topProductsRating;
        }

        public List<ProductView> GetProductsOffer(DataUser dataUser, int count)
        {
            List<ProductView> productsOffer = _productRepository.GetProductsOffer(count);
            //if (productsOffer != null)
            //{
            //    foreach (ProductView productView in productsOffer)
            //    {
            //        productView.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), productView.Slug, productView.Image);
            //    }
            //}
            return productsOffer;
        }

        public List<ProductView> GetProductsTopSale(DataUser dataUser, int count)
        {
            List<ProductView> productsOffer = _productRepository.GetProductsTopSale(count);
            //if (productsOffer != null)
            //{
            //    foreach (ProductView productView in productsOffer)
            //    {
            //        productView.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), productView.Slug, productView.Image);
            //    }
            //}
            return productsOffer;
        }

        public List<ProductView> GetLatestProductsViewUser(int count, DataUser dataUser)
        {
            count = count > 0 ? count : 4;
            List<ProductView> productViews = _productRepository.GetLatestProductsViewUser(count);
            //if (productViews != null)
            //{
            //    foreach (ProductView productView in productViews)
            //    {
            //        productView.Image = HelperFile.SourceImage(ApplicationSettingsAccessor.GetDirectoryImages(), productView.Slug, productView.Image);
            //    }
            //}
            return productViews;
        }

        public int SaveSlideImage(byte[] bytes, int slideId, string extension, DataUser dataUser)
        {
            Slide slide = _slideRepository.GetSlide(slideId);
            int output = 0;
            if (slide != null)
            {
                string dir = ApplicationSettingsAccessor.GetDirectorySlides();
                string fullPath = System.IO.Path.Combine(dir, $"{slideId}{extension}");
                System.IO.File.WriteAllBytes(fullPath, bytes);
                slide.Image = System.IO.Path.GetFileName(fullPath);
                //slide.Image = $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}";
                //slide.Image = Convert.ToBase64String(bytes);
                // output = _slideRepository.Save();

                Result<Slide> result = _slideRepository.Update(slide);
                output = result.Success ? 1 : 0;
            }
            return output;
        }
    }
}
