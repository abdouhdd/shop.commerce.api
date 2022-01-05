using Microsoft.Extensions.Logging;
using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Models;
using shop.commerce.api.domain.Models.Account;
using shop.commerce.api.infrastructure.Repositories;
using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.infrastructure.Repositories.EntityFramework;
using shop.commerce.api.services.Helpers;
using System;
using System.Collections.Generic;

namespace shop.commerce.api.services.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly ILogger<AccountService> _logger;
        private readonly MessagesHelper _messagesHelper;

        public AccountService(IUserRepository accountRepository, IAdminRepository adminRepository, ILogger<AccountService> logger, MessagesHelper messagesHelper)
        {
            _messagesHelper = messagesHelper;
            _userRepository = accountRepository;
            _adminRepository = adminRepository;
            _logger = logger;
        }

        public MyResult<ProfileModel> AuthenticateAdmin(AuthenticationRequest authenticationRequest)
        {
            MyResult<ProfileModel> result = null;
            try
            {
                _logger.LogDebug($"AuthenticateAdmin => email: {authenticationRequest?.Email}");

                //var admins = _accountRepository.FindAdmins();
                Admin admin = _adminRepository.GetFirst((req) => req.AddPredicate(a => a.Email == authenticationRequest.Email));

                int timeDesactivation = 120;

                if (admin != null)
                {
                    if (admin.Status == EnumStatusAccount.Bloque)
                    {
                        result = MyResult<ProfileModel>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.UserBloque), MyResultCode.UserBloque);
                    }
                    else if (admin.Status == EnumStatusAccount.Desactive)
                    {
                        if (admin.DateDesactive.HasValue)
                        {
                            var diffDate = DateTime.Now - admin.DateDesactive.Value;
                            int timeReste = timeDesactivation - (int)diffDate.TotalSeconds;
                            if (timeReste <= 0)
                            {

                                _adminRepository.Update(admin.Id, (a) =>
                                {
                                    a.DateDesactive = null;
                                    a.Status = EnumStatusAccount.Active;
                                    a.CountFailLogin = 0;
                                });
                            }
                            else
                            {
                                string message = _messagesHelper.GetMessageCode(MyResultCode.UserDesactiveEnTime).Replace("{time}", TimeSpan.FromSeconds(timeReste).ToString(@"mm\:ss"));
                                result = MyResult<ProfileModel>.ResultError(null, message, MyResultCode.UserDesactiveEnTime);
                            }
                        }
                        else
                        {
                            result = MyResult<ProfileModel>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.UserDesactiveEnTime), MyResultCode.UserDesactiveEnTime);
                        }
                        //throw new CustomException(GlobalConfiguration.Mensajes.UsuarioDesactivado);
                    }
                    if (admin.Status == EnumStatusAccount.Active)
                    {
                        HashMD5 mD5 = new HashMD5();
                        if (mD5.VerifyMd5Hash(authenticationRequest.Password, admin.PasswordHash))
                        {
                            result = MyResult<ProfileModel>.ResultSuccess(new ProfileModel
                            {
                                UserName = admin.Username,
                                Email = authenticationRequest.Email,
                                Role = EnumRole.Admin,
                            });
                        }
                        else
                        {

                            string code;
                            string message;
                            admin.CountFailLogin++;

                            if (admin.CountFailLogin < 4)
                            {
                                code = MyResultCode.InvalidPassword;
                                message = _messagesHelper.GetMessageCode(code).Replace("{Tentatives}", (4 - admin.CountFailLogin).ToString());
                            }
                            else
                            {


                                _adminRepository.Update(admin.Id, (a) =>
                                {
                                    a.Status = EnumStatusAccount.Desactive;
                                    a.CountDesactive++;
                                    a.DateDesactive = DateTime.Now;
                                });

                                code = MyResultCode.UserDesactiveEnTime;
                                //result.Message = "Correo electrónico bloqueado, póngase en contacto con su administrador";
                                message = _messagesHelper.GetMessageCode(code).Replace("{time}", TimeSpan.FromSeconds(timeDesactivation).ToString(@"mm\:ss"));

                            }

                            result = MyResult<ProfileModel>.ResultError(null, message, code);
                        }
                    }
                    //if(result == null)
                    //{
                    //    result = Result<ProfileModel>.ResultError(null, _messagesHelper.GetMessageCode(ResultCode.UserDesactiveEnTime), ResultCode.UserDesactiveEnTime);
                    //}
                }
                else
                {
                    result = MyResult<ProfileModel>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.AdminNoExiste), MyResultCode.AdminNoExiste);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                result = MyResult<ProfileModel>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.ErrorCode), MyResultCode.ErrorCode);
            }
            return result;
        }

        public MyResult<ProfileModel> AuthenticateUser(AuthenticationRequest authenticationRequest)
        {
            MyResult<ProfileModel> result;
            try
            {
                User user = _userRepository.FindUserByEmail(authenticationRequest.Email);
                if (user != null)
                {
                    HashMD5 mD5 = new HashMD5();
                    if (mD5.VerifyMd5Hash(authenticationRequest.Password, user.PasswordHash))
                    {
                        result = MyResult<ProfileModel>.ResultSuccess(new ProfileModel
                        {
                            UserName = user.Username,
                            Email = authenticationRequest.Email,
                            Role = EnumRole.User,
                        });
                    }
                    else
                    {
                        result = MyResult<ProfileModel>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.UserOrPasswordInvalid), MyResultCode.UserOrPasswordInvalid);
                    }
                }
                else
                {
                    result = MyResult<ProfileModel>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.UserNoExiste), MyResultCode.UserNoExiste);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                result = MyResult<ProfileModel>.ResultError(null, _messagesHelper.GetMessageCode(MyResultCode.ErrorCode), MyResultCode.ErrorCode);
            }
            return result;
        }

        public IEnumerable<User> FindUsers()
        {
            try
            {
                return _userRepository.FindUsers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public IEnumerable<Admin> FindAdmins()
        {
            try
            {
                return _adminRepository.FindAdmins();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public int SaveUsers(User[] users)
        {
            int output = 0;
            try
            {
                foreach (User user in users)
                {
                    user.Role = EnumRole.User;
                    user.RegistrationDate = DateTime.UtcNow;
                    HashMD5 hashMD5 = new HashMD5();
                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        user.PasswordHash = hashMD5.GetMd5Hash(user.PasswordHash);
                    }
                }
                output = _userRepository.SaveUsers(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return output;
        }

        public int SaveAdmins(Admin[] admins)
        {
            try
            {
                foreach (Admin admin in admins)
                {
                    admin.Role = EnumRole.Admin;
                    admin.RegistrationDate = DateTime.UtcNow;

                    HashMD5 hashMD5 = new HashMD5();
                    if (!string.IsNullOrEmpty(admin.PasswordHash))
                    {
                        admin.PasswordHash = hashMD5.GetMd5Hash(admin.PasswordHash);
                    }
                }

                common.Result output = _adminRepository.AddRange(admins);
                return output.Status == common.ResultStatus.Succeed ? 1 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return 0;
            }
        }

        public Admin FindAdminByEmail(string email)
        {
            try
            {
                return _adminRepository.FindAdminByEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public Admin FindAdminByUsername(string username)
        {
            try
            {
                return _adminRepository.FindAdminByUsername(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public bool EnsureCreated()
        {
            try
            {
                bool created = _userRepository.EnsureCreated();
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public bool EnsureDeleted()
        {
            try
            {
                bool created = _userRepository.EnsureDeleted();
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
