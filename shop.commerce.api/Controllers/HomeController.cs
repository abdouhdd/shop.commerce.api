using shop.commerce.api.Application.Configuration;
using shop.commerce.api.domain.Views;
using shop.commerce.api.services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using shop.commerce.api.domain.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.IO;
//using Microsoft.Extensions.DependencyInjection;
using shop.commerce.api.infrastructure.Repositories.Entities;
using Microsoft.AspNetCore.Authorization;

namespace shop.commerce.api.presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerCore
    {
        private readonly IUserService _userService;
        private readonly IHomeService _homeService;

        public HomeController(IUserService userService, IHomeService homeService, IApplicationSettingsAccessor applicationSettingsAccessor)
        {
            _userService = userService;
            _homeService = homeService;
            _userService.ApplicationSettingsAccessor = applicationSettingsAccessor;
            _homeService.ApplicationSettingsAccessor = applicationSettingsAccessor;
        }

        [HttpPost("GetSlideData")]
        public ActionResult GetSlideData(bool all = true, bool active = true)
        {
            List<SlideView> result = _homeService.GetSlideData(all, active, DataUser);
            return Ok(result);
        }
        
        [Authorize]
        [HttpPost("InsertSlide")]
        public ActionResult InsertSlide(SlideView slide)
        {
            MyResult<SlideView> result = _homeService.InsertSlide(slide, DataUser);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("UpdateSlide")]
        public ActionResult UpdateSlide(SlideView slide)
        {
            MyResult<SlideView> result = _homeService.UpdateSlide(slide, DataUser);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("UploadSlideImage")]
        public ActionResult UploadSlideImage(List<IFormFile> photos, int slideId)
        {
            //var applicationSettingsAccessor = HttpContext.RequestServices.GetService<IApplicationSettingsAccessor>();
            try
            {
                if (photos.Count == 1 && slideId > 0)
                {
                    IFormFile formFile = photos[0];
                    string filename = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        formFile.CopyTo(memoryStream);
                        slideId = _homeService.SaveSlideImage(memoryStream.ToArray(), slideId, Path.GetExtension(filename), DataUser);
                    }
                    return Ok(new { success = true, message = $"{slideId} updated" });
                }
                else
                {
                    return Ok(new { success = false, message = "Slide image obligatoire" });
                }
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("RemovetSlide")]
        public ActionResult RemovetSlide(int slideId)
        {
            MyResult<int> result = _homeService.RemoveSlide(slideId, DataUser);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("UpSlide")]
        public ActionResult UpSlide(int slideId)
        {
            MyResult<int> result = _homeService.UpSlide(slideId, DataUser);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("DownSlide")]
        public ActionResult DownSlide(int slideId)
        {
            MyResult<int> result = _homeService.DownSlide(slideId, DataUser);
            return Ok(result);
        }

        [HttpPost("GetLastSlideView")]
        public ActionResult GetLastSlideView()
        {
            SlideView slideView = _homeService.GetLastSlideView(DataUser);
            return Ok(slideView);
        }

        [HttpPost("GetSlide")]
        public ActionResult GetSlide(int slideId)
        {
            Slide result = _homeService.GetSlide(slideId, DataUser);
            return Ok(result);
        }
        
        [HttpPost("GetProductsTopRating")]
        public ActionResult GetProductsTopRating(int count = 6)
        {
            List<ProductView> result = _homeService.GetProductsTopRating(DataUser, count);
            return Ok(result);
        }
        
        [HttpPost("GetProductsOffer")]
        public ActionResult GetProductsOffer(int count = 6)
        {
            List<ProductView> result = _homeService.GetProductsOffer(DataUser, count);
            return Ok(result);
        }

        [HttpPost("GetProductsTopSale")]
        public ActionResult GetProductsTopSale(int count = 4)
        {
            List<ProductView> result = _homeService.GetProductsTopSale(DataUser, count);
            return Ok(result);
        }

        [HttpPost("GetLatestProductsViewUser")]
        public ActionResult GetLatestProductsViewUser(int count = 3)
        {
            List<ProductView> result = _homeService.GetLatestProductsViewUser(count, DataUser);
            return Ok(result);
        }
    }
}
