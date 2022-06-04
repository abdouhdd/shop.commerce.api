using shop.commerce.api.Application.Configuration;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Response;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.services.Helpers;
using shop.commerce.api.services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Models.Request;

namespace shop.commerce.api.presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerCore
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService, IApplicationSettingsAccessor applicationSettingsAccessor)
        {
            _adminService = adminService;
            _adminService.ApplicationSettingsAccessor = applicationSettingsAccessor;
        }

        [HttpPost("OrderProducts")]
        public ActionResult OrderProducts(bool force)
        {
            return Ok(_adminService.OrderProducts(force));
        }

        [HttpPost("UpProduct")]
        public ActionResult UpProduct(string slug)
        {
            return Ok(_adminService.UpProduct(slug));
        }
        
        [HttpPost("DownProduct")]
        public ActionResult DownProduct(string slug)
        {
            return Ok(_adminService.DownProduct(slug));
        }

        [HttpPost("CreateProduct")]
        public ActionResult CreateProduct(ProductRequest product)
        {
            MyResult<ProductResponse> result = _adminService.CreateProduct(product, DataUser);
            return ActionResultFor(result);
        }
        
        [HttpPost("UpdateProduct")]
        public ActionResult UpdateProduct(ProductRequest product)
        {
            MyResult<ProductResponse> result = _adminService.UpdateProduct(product, DataUser);
            return ActionResultFor(result);
        }

        [HttpPost("UploadImages")]
        public ActionResult UploadImages(List<IFormFile> photos, string slug)
        {
            var applicationSettingsAccessor = HttpContext.RequestServices.GetService<IApplicationSettingsAccessor>();

            string[] files = new string[photos.Count];
            //string guid = Guid.NewGuid().ToString("n").Substring(0, 10);
            string directory = Path.Combine(applicationSettingsAccessor.GetDirectoryImages(), slug);
            int countImages = 0;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                countImages = Directory.GetFiles(directory).Length;
            }
            for (int i = 0; i < photos.Count; i++)
            {
                IFormFile formFile = photos[i];
                string filename = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                filename = $"{i+1+countImages}{Path.GetExtension(filename)}";
                string fullPath = HelperFile.FullPathImage(directory, filename);
                files[i] = Path.GetFileName(fullPath);
                
                using FileStream output = System.IO.File.Create(fullPath);
                formFile.CopyTo(output);
            }
            _adminService.SaveImages(files, slug, DataUser);
            //return Ok(new { FilesName = files, Guid = guid });
            return Ok(new { files });
        }

        [HttpPost("UpdateImages")]
        public ActionResult UpdateImages(List<ImageView> images, string slug)
        {
            var result = _adminService.UpdateImages(slug, images, DataUser);
            return Ok(result);
        }

        [HttpPost("RemoveProduct")]
        public ActionResult RemoveProduct(string slug)
        {
            MyResult<int> result = _adminService.DesactivateProduct(slug, DataUser);
            return Ok(result);
        }
        
        [HttpPost("GetProductEdit")]
        public ActionResult GetProductEdit(string slug)
        {
            ProductEdit result = _adminService.GetProductEdit(slug, DataUser);
            return Ok(result);
        }

        [HttpPost("RemoveImage")]
        public ActionResult RemoveImage(string imageGuid, string slug)
        {
            MyResult<int> result = _adminService.RemoveImage(slug, imageGuid, DataUser);
            return Ok(result);
        }

        [HttpPost("GetProductDetailView")]
        public ActionResult GetProductDetailView(string slug)
        {
            ProductDetailView productDetailView = _adminService.GetProductDetailView(slug, DataUser);
            return Ok(productDetailView);
        }

        [HttpPost("GetProductsView")]
        public ActionResult GetProductsView(ProductFilterAdmin productFilter)
        {
            List<ProductView> result = _adminService.GetProductsView(productFilter, DataUser);
            return Ok(result);
        }

        [HttpPost("GetProductsViewPage")]
        public ActionResult GetProductsViewPage(ProductFilterAdmin productFilter)
        {
            ResultPage<ProductView> productViews = _adminService.GetProductsViewPage(productFilter, DataUser);
            return Ok(productViews);
        }

        [HttpPost("GetOrdersView")]
        public ActionResult GetOrdersView(OrderFilterAdmin orderFilterAdmin)
        {
            IEnumerable<OrderView> result = _adminService.GetOrdersView(orderFilterAdmin, DataUser);
            return Ok(result);
        }

        [HttpPost("GetOrderView")]
        public ActionResult GetOrderView(string orderNumber)
        {
            OrderView order = _adminService.GetOrderView(orderNumber);
            return Ok(order);
        }

        [HttpPost("GetOrderDetailsView")]
        public OrderItemView GetOrderDetailsView(string orderItemNumber)
        {
            OrderItemView orderItemView = _adminService.GetOrderDetailsView(orderItemNumber);
            return orderItemView;
        }

        [HttpPost("UpdateOrderStatus")]
        public ActionResult UpdateOrderStatus(string orderNumber, EnumOrderStatus status)
        {
            var orders = _adminService.UpdateOrderStatus(orderNumber, status, DataUser);
            return Ok(orders);
        }

        [HttpPost("GetOrderTrackings")]
        public ActionResult GetOrderTrackings(string orderNumber)
        {
            var orderTrackings = _adminService.GetOrderTrackings(orderNumber, DataUser);
            return Ok(orderTrackings);
        }

        [HttpPost("CreateSeller")]
        public ActionResult CreateSeller(AdminPutModel model)
        {
            _adminService.CreateSeller(model, DataUser);
            return Ok();
        }
    }
}
