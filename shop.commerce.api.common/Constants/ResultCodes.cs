namespace shop.commerce.api.common
{
    /// <summary>
    /// a list of result codes
    /// </summary>
    public static class ResultCode
    {
        /// <summary>
        /// this code represent that the Operation has succeed
        /// </summary>
        public const string OperationSucceeded = "operation_succeeded";

        /// <summary>
        /// this code represent that the Operation has failed
        /// </summary>
        public const string OperationFailed = "operation_failed";

        /// <summary>
        /// this code represent that the Operation has failed because of a custom exception
        /// </summary>
        public const string OperationFailedException = "operation_failed_exception";

        /// <summary>
        /// this code represent that an internal error has accrued while processing the request
        /// </summary>
        public const string InternalError = "internal_error";

        /// <summary>
        /// this code represent that an internal exception has accrued while processing the request
        /// </summary>
        public const string InternalException = "internal_exception";

        /// <summary>
        /// the user is unauthorized to perform the action
        /// </summary>
        public const string Unauthorized = "unauthorized";

        /// <summary>
        /// the requested resource cannot be found
        /// </summary>
        public const string NotFound = "not_found";

        /// <summary>
        /// the user has entered invalid login credentials
        /// </summary>
        public const string InvalidLoginCredentials = "invalid_login_credentials";

        /// <summary>
        /// the given resource already exist 
        /// </summary>
        public const string ResourceAlreadyExist = "resource_already_exist";

        /// <summary>
        /// this code that a validation of the resource has failed
        /// </summary>
        public const string ValidationFailed = "validation_failed";

        /// <summary>
        /// this code means that we failed to save the data to the database
        /// </summary>
        public const string DataPersistenceFailed = "data_persistence_failed";

        /// <summary>
        /// this code means that resource has been processed, and no need for re processing
        /// </summary>
        public const string ResourceAlreadyProcessed = "resource_already_processed";

        /// <summary>
        /// this code means that resource has been Canceled, cannot be processed
        /// </summary>
        public const string ResourceCanceled = "resource_canceled";

        /// <summary>
        /// this code means that the authentication failed
        /// </summary>
        public const string AuthenticationFailed = "authentication_failed";

        /// <summary>
        /// this code indicate that we couldn't reach the service, the connection has failed
        /// </summary>
        public const string ServiceConnectionFailed = "service_connection_failed";

        /// <summary>
        /// this code indicate that the value is required
        /// </summary>
        public const string PasswordRequired = "password_required";

        /// <summary>
        /// this code indicate that the given email value is not valid
        /// </summary>
        public static string InvalidEmail = "invalid_email";

        /// <summary>
        /// this code indicate that the value email value is required
        /// </summary>
        public static string EmailRequired = "email_required";

        /// <summary>
        /// this code indicate that the file name is required
        /// </summary>
        public const string FileNameRequired = "file_name_required";

        /// <summary>
        /// this code indicate that the file content is required
        /// </summary>
        public const string FileContentRequired = "file_content_required";

        /// <summary>
        /// the given operation type is not supported
        /// </summary>
        public const string UnsupportedActionType = "un_supported_action_type";

        /// <summary>
        /// this code means that we couldn't find a user with the given email
        /// </summary>
        public const string UserWithEmailNotExist = "user_with_email_not_exist";

        /// <summary>
        /// this code means that you have supplier an invalid password for your login
        /// </summary>
        public const string InvalidUserPassword = "invalid_user_password";

        /// <summary>
        /// this code means that the user account has been blocked
        /// </summary>
        public const string UserAccountBlocked = "user_account_blocked";

        /// <summary>
        /// this code means that the user account has been Deactivated
        /// </summary>
        public const string UserAccountDesactivated = "user_account_desactivated";

        /// <summary>
        /// this code means that the user company has been blocked
        /// </summary>
        public const string UserCompanyBlocked = "user_company_blocked";

        /// <summary>
        /// this code means that the user company has been Deactivated
        /// </summary>
        public const string UserCompanyDeactivated = "user_company_deactivated";

        
    }
}
