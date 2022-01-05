
namespace shop.commerce.api.Application.Models
{
    /// <summary>
    /// this class hold the global application settings extracted from IConfiguration in the appSettings.json file
    /// any changes to the AppSetting file should be applied here also
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// the images output directory
        /// </summary>
        public string DirectoryImages { get; set; }
        
        public string DirectorySlidesImages { get; set; }

        /// <summary>
        /// the authentication settings
        /// </summary>
        public AuthenticationSettings Authentication { get; set; }
        public bool IsMemoryDatabase { get; set; }
    }

    /// <summary>
    /// the email settings
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// the Credential Email
        /// </summary>
        public string CredentialsEmail { get; set; }

        /// <summary>
        /// the credential password
        /// </summary>
        public string CredentialsPass { get; set; }

        /// <summary>
        /// is SSL enabled
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// is HTML body supported
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// the port 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// the SMTP Server
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// should use Default Credentials
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// the From Email Address
        /// </summary>
        public string SystemNotificationEmail_From { get; set; }

        /// <summary>
        /// the To email
        /// </summary>
        public string SystemNotificationEmail_To { get; set; }
    }

    /// <summary>
    /// the authentication Settings
    /// </summary>
    public class AuthenticationSettings
    {
        /// <summary>
        ///  Gets or sets the number of failed access attempts allowed before a user is locked
        ///  out, assuming lock out is enabled. Defaults to 5.
        /// </summary>
        public int MaxFailedAccessAttempts { get; set; } = 5;

        /// <summary>
        /// Gets or sets the time in minutes a user is locked out for when a lockout occurs. Defaults to 5 minutes.
        /// </summary>
        public int DefaultLockoutTimeSpan { get; set; } = 5;
    }
}
