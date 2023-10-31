using BackOffice.Authorizer.Management.Utilities;
using IDP.Common.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Password { 
        private static readonly Regex PasswordRegex = new Regex(@"^(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*\-_\<\>\\\/\|]).{8,}$", RegexOptions.Compiled);
        private static readonly ICryptographyStrategy cryptographyStrategy = new RijndaelCryptographyStrategy(); 
        public static Password FromDecripted(string encryptedPassword) { var decryptedPassword = cryptographyStrategy.Decrypt(encryptedPassword); return new Password(decryptedPassword); } 
        public static Password FromEncrypted(Password password) { var encryptedPassword = cryptographyStrategy.Encrypt(password.Value); return new Password(encryptedPassword); } 
        public static Password FromString(string value) { if (!PasswordRegex.IsMatch(value)) { throw new ArgumentException(Message.ForKey("Invalid.Password", new string[] { }).Label); } return new Password(value); } 
        public string Value { get; private set; } private Password(string value) { Value = value; } }
}
