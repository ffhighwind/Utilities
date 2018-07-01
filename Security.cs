using System;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Utilities
{
    /// <summary>
    /// Security measures for encrypting and decrypting data.
    /// </summary>
    /// <source> https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file </source>
    public static class Security
    {
        private static readonly Encoding encoding = Encoding.Unicode;
        private static readonly byte[] entropy = Encoding.Unicode.GetBytes("TY2JY+hhs[R7y+v_");

        /// <summary>
        /// Encrypts data such as passwords using Windows DPAPI.
        /// </summary>
        /// <param name="input">The string containing the data to encrypt.</param>
        /// <param name="scope">CurrentUser means that the data can only be decrypted if the user is logged in. 
        /// LocalMachine means that any user on the machine can decrypt the data.</param>
        /// <returns>The encrypted data.</returns>
        public static string Encrypt(SecureString input, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            return Encrypt(UnsecureString(input), scope);
        }

        /// <summary>
        /// Encrypts data such as passwords using Windows DPAPI.
        /// </summary>
        /// <param name="input">The string containing the data to encrypt.</param>
        /// <param name="scope">CurrentUser means that the data can only be decrypted if the user is logged in. 
        /// LocalMachine means that any user on the machine can decrypt the data.</param>
        /// <returns>The encrypted data.</returns>
        public static string Encrypt(string input, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            byte[] encryptedData = ProtectedData.Protect(
                encoding.GetBytes(input),
                entropy,
                scope);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts data such as passwords using the Windows DPAPI.
        /// </summary>
        /// <param name="encryptedData">The data to decrypt.</param>
        /// <param name="scope">CurrentUser means that the data can only be decrypted if the user is logged in. 
        /// LocalMachine means that any user on the machine can decrypt the data.</param>
        /// <returns>The decrypted data.</returns>
        public static string Decrypt(string encryptedData, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            try {
                byte[] decryptedData = ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    scope);
                return encoding.GetString(decryptedData);
            }
            catch {
                return null;
            }
        }

        /// <summary>
        /// Encrypts a string and makes it immutable. This ensures that the data is secure in memory from malicious processes.
        /// </summary>
        /// <param name="input">The string to encrypt.</param>
        /// <returns>An encrypted immutable SecureString.</returns>
        public static SecureString SecureString(string input)
        {
            SecureString result = new SecureString();
            foreach (char c in input) {
                result.AppendChar(c);
            }
            result.MakeReadOnly();
            return result;
        }

        /// <summary>
        /// Decrypts a SecureString.
        /// </summary>
        /// <param name="input">The SecureString to decrypt.</param>
        /// <returns></returns>
        public static string UnsecureString(this SecureString input)
        {
            IntPtr ptr = Marshal.SecureStringToBSTR(input);
            try {
                return Marshal.PtrToStringBSTR(ptr);
            }
            catch {
                Marshal.ZeroFreeBSTR(ptr); //FAILED TO DECRYPT! Zero out the data and free it.
            }
            return null;
        }

        /// <summary>
        /// Geneates an encrypted section in the app.config file. This should only be used once before deployment.
        /// </summary>
        /// <param name="sectionKey">The section to encrypt.</param>
        private static void EncryptConfig(string sectionKey = "configuration")
        {
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            System.Configuration.ConfigurationSection section = config.GetSection(sectionKey);
            if (section != null) {
                if (!section.SectionInformation.IsProtected) {
                    if (!section.ElementInformation.IsLocked) {
                        section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                        section.SectionInformation.ForceSave = true;
                        config.Save(System.Configuration.ConfigurationSaveMode.Full);
                    }
                }
            }
        }
    }
}
