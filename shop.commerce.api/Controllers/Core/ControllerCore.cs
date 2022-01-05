using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Models;
using shop.commerce.api.services.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System;

namespace shop.commerce.api.presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerCore : ControllerBase
    {
        protected ActionResult ActionResultFor<TResult>(MyResult<TResult> result)
        {
            if (result.Success)
            {
                return Ok(result);
            }
            if (result.Data == null)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        private DataUser dataUser;
        protected DataUser DataUser
        {
            get
            {
                ClaimsIdentity claimsIdentity = User.Identity as ClaimsIdentity;
                List<Claim> claims = claimsIdentity.Claims.ToList();
                if (claims.Count > 0)
                {
                    dataUser = new DataUser
                    {
                        Username = claims[0].Value,
                        Role = (EnumRole)Enum.Parse(typeof(EnumRole), claims[1].Value)
                    };
                }
                return dataUser;
            }
        }

        protected string IpNavegador()
        {
            string ip = HttpContext.Connection.RemoteIpAddress.ToString();
            return ip;
        }

        protected string Navegador()
        {
            // OperacionsFirmante/DetalleOperacion
            //string ipNavegador = HttpContext.Connection.RemoteIpAddress.ToString();
            //if (_httpContextAccessor.HttpContext.Request.Headers != null)
            //{
            //    //the X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a client
            //    //connecting to a web server through an HTTP proxy or load balancer
            //    var forwardedHeader = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"];
            //    if (!StringValues.IsNullOrEmpty(forwardedHeader))
            //        ipNavegador = forwardedHeader.FirstOrDefault();
            //}

            ////if this header not exists try get connection remote IP address
            //if (string.IsNullOrEmpty(ipNavegador) && _httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null)
            //    ipNavegador = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            string navegador = "";
            return navegador;
        }
    }
}
