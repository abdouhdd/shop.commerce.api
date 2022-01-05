using shop.commerce.api.Application.Configuration;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Account;
using shop.commerce.api.services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace shop.commerce.api.presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerCore
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("AuthenticateAdmin")]
        public ActionResult AuthenticateAdmin(AuthenticationRequest authenticationRequest)
        {
            MyResult<ProfileModel> profile = _accountService.AuthenticateAdmin(authenticationRequest);
            if (profile != null && profile.Data != null)
            {
                var applicationSecretsAccessor = HttpContext.RequestServices.GetService<IApplicationSecretsAccessor>();

                profile.Data.Token = GenerateJwtToken(applicationSecretsAccessor.GetAuthenticationSecrets().Secret, profile.Data);
            }
            return ActionResultFor(profile);
        }

        [HttpPost("IsConnecte")]
        public ActionResult IsConnecte()
        {
            if (DataUser == null)
            {
                return Ok(new { Success = false });
            }
            else
            {
                return Ok(new { Success = true });
            }
        }

        private string GenerateJwtToken(string secret, ProfileModel user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Username", user.UserName),
                    new Claim("Role", user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

    }
}
