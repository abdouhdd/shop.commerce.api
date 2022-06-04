using System;

namespace shop.commerce.api.domain.Models
{
    public class MyResultCode
    {
        public const string ErrorCode = "error";
        public const string SuccessCode = "success";

        public const string AdminNoExiste = "AdminNoExiste";
        public const string UserNoExiste = "UserNoExiste";
        public const string SellerNoExiste = "SellerNoExiste";
        public const string CategoryInsertedSuccess = "CategoryInsertedSuccess";
        public const string CategoryInsertedError = "CategoryInsertedError";
        public const string CategoryUpdatedSuccess = "CategoryUpdatedSuccess";
        public const string CategoryUpdatedError = "CategoryUpdatedError";
        public const string CategoryObligatory = "CategoryObligatory";
        public const string CategoryNotExist = "CategoryNotExist";
        public const string CategoryAlreadyExist = "CategoryAlreadyExist";
        public const string CategoryDeletedSuccess = "CategoryDeletedSuccess";
        public const string CategoryDeletedFailed = "CategoryDeletedFailed";
        public const string SlugInvalid = "SlugInvalid";
        public const string CategoryDesactivated = "CategoryDesactivated";
        public const string CategoryActivated = "CategoryActivated";
        public const string OrdreCreateError = "OrdreCreateError";
        public const string StatusOrderInvalid = "StatusOrderInvalid";
        public const string RemoveProductError = "RemoveProductError";
        public const string RemoveProductImageError = "RemoveProductImageError";

        public const string InsertSlideError = "InsertSlideError";
        public const string UpdateSlideError = "UpdateSlideError";
        public const string RemoveSlideError = "RemoveSlideError";
        
        public const string UpSlideError = "UpSlideError";
        public const string DownSlideError = "DownSlideError";

        public const string UpdateProductError = "UpdateProductError";

        public const string ProductNameRequired = "ProductNameRequired";
        public const string SlugAlreadyExists = "SlugAlreadyExists";
        public const string UserOrPasswordInvalid = "UserOrPasswordInvalid";
        public const string InvalidPassword = "InvalidPassword";
        public const string UserDesactiveEnTime = "UserDesactiveEnTime";
        public const string UserBloque = "UserBloque";
        public const string CreateSellerExiste = "CreateSellerExiste";

    }
}
