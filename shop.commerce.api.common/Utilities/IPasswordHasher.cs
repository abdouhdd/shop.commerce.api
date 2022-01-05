namespace shop.commerce.api.common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// the password hasher service
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// check if the user hashed password equals the given password
        /// </summary>
        /// <param name="passwordHash">the hashed Password</param>
        /// <param name="slat">the slat</param>
        /// <param name="password">the password to be checked</param>
        /// <returns>true if valid false if not</returns>
        bool CheckUserPassword(string passwordHash, string slat, string password);

        /// <summary>
        /// hash the given password
        /// </summary>
        /// <param name="password">the password to be hashed</param>
        /// <param name="slat">the slat</param>
        /// <returns>the hashed password</returns>
        string HashPassword(string password, string slat);
    }

    /// <summary>
    /// service implementation for <see cref="IPasswordHasher"/>
    /// </summary>
    public partial class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// check if the user hashed password equals the given password
        /// </summary>
        /// <param name="passwordHash">the hashed Password</param>
        /// <param name="slat">the slat</param>
        /// <param name="password">the password to be checked</param>
        /// <returns>true if valid false if not</returns>
        public bool CheckUserPassword(string passwordHash, string slat, string password)
            => Cryptography.Decrypt(passwordHash, slat) == password;

        /// <summary>
        /// hash the given password
        /// </summary>
        /// <param name="password">the password to be hashed</param>
        /// <param name="slat">the slat</param>
        /// <returns>the hashed password</returns>
        public string HashPassword(string password, string slat)
            => Cryptography.Encrypt(password, slat);
    }
}
