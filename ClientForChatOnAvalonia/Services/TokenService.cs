using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Avalonia.Platform.Storage;
using ClientForChatOnAvalonia.Interfaces.Services;

namespace ClientForChatOnAvalonia.Services
{
    public class TokenService: ITokenService
    {
        private const string TokenFileName = "token.dat";

        public void SaveToken(string token)
        {
            var encryptedData = ProtectData(Encoding.UTF8.GetBytes(token));
            File.WriteAllBytes(GetTokenFilePath(), encryptedData);
        }

        public string GetToken()
        {
            var filePath = GetTokenFilePath();
            if (!File.Exists(filePath)) return null;

            var encryptedData = File.ReadAllBytes(filePath);
            var decryptedData = UnprotectData(encryptedData);
            return Encoding.UTF8.GetString(decryptedData);
        }

        public void DeleteToken()
        {
            var filePath = GetTokenFilePath();
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private string GetTokenFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, TokenFileName);
        }

        private byte[] ProtectData(byte[] data)
        {
            return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        }

        private byte[] UnprotectData(byte[] data)
        {
            return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
        }
    }
}
