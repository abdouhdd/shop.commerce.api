using shop.commerce.api.Application.Configuration;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Views;
using shop.commerce.api.services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using shop.commerce.api.domain.Models.Request;
using shop.commerce.api.domain.Models;

namespace shop.commerce.api.presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerCore
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService, IApplicationSettingsAccessor applicationSettingsAccessor)
        {
            _userService = userService;
            _userService.ApplicationSettingsAccessor = applicationSettingsAccessor;
        }

        [HttpGet("Image")]
        public ActionResult Image(string slug, string file)
        {
            try
            {
                //string fullpath = $"images/products/{file}";
                //string fullpath = $"ClientApp/images/img-1.png";
                string directory = _userService.ApplicationSettingsAccessor.GetDirectoryImages();
                string fullpath = $"{directory}/{slug}/{file}";
                if (!System.IO.File.Exists(fullpath))
                {
                    fullpath = $"{directory}/images/{file}";
                }
                if (System.IO.File.Exists(fullpath))
                {
                    return File(System.IO.File.ReadAllBytes(fullpath), "image/*", file);
                }
                else
                {
                    return Ok("");
                }
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }
        
        [HttpGet("ImageSlide")]
        public ActionResult ImageSlide(string image)
        {
            try
            {
                //string directory = _userService.ApplicationSettingsAccessor.GetDirectoryImages();
                string directorySlides = _userService.ApplicationSettingsAccessor.GetDirectorySlides();
                string fullpath = System.IO.Path.Combine(directorySlides,image);
                if (System.IO.File.Exists(fullpath))
                {
                    return File(System.IO.File.ReadAllBytes(fullpath), "image/*");
                }
                else
                {
                    return Ok("");
                }
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("GetProductsView")]
        public ActionResult GetProductsView(ProductFilterUser productFilter)
        {
            try
            {
                List<ProductView> result = _userService.GetProductsView(productFilter, DataUser);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }
        
        [HttpPost("GetProductsViewPage")]
        public ActionResult GetProductsViewPage(ProductFilterUser productFilter)
        {
            try
            {
                ResultPage<ProductView> result = _userService.GetProductsViewPage(productFilter, DataUser);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("GetProductDetailView")]
        public ActionResult GetProductDetailView(string slug)
        {
            try
            {
                ProductDetailView productDetailView = _userService.GetProductDetailView(slug, DataUser);
                return Ok(productDetailView);
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("GetRelatedProductsBySlug")]
        public ActionResult GetRelatedProductsBySlug(string slug, int count)
        {
            try
            {
                List<ProductView> result = _userService.GetRelatedProductsBySlug(slug, count, DataUser);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("GetRelatedProductsByCategory")]
        public ActionResult GetRelatedProductsByCategory(int categoryId, int count)
        {
            try
            {
                List<ProductView> result = _userService.GetRelatedProductsByCategory(categoryId, count, DataUser);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("CreateOrder")]
        public ActionResult CreateOrder(OrderRequest orderRequest)
        {
            try
            {
                orderRequest.AddressIp = base.IpNavegador();
                orderRequest.Browser = base.Navegador();
                MyResult<OrderView> result = _userService.CreateOrder(orderRequest, DataUser);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("GetPaymentMethods")]
        public ActionResult GetPaymentMethods()
        {
            try
            {
                return Ok(_userService.GetPaymentMethods(DataUser));
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("GetOrderStatus")]
        public ActionResult GetOrderStatus()
        {
            try
            {
                return Ok(_userService.GetOrderStatus(DataUser));
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

    }
}
