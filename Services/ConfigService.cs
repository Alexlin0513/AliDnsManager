using System;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace AliDnsManager.Services
{
    public class ConfigService
    {
        private readonly string _configPath;
        private readonly byte[] _entropy = Encoding.UTF8.GetBytes("AliDnsManager");

        public ConfigService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "AliDnsManager");
            Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, "config.json");
        }

        public void SaveConfig(string accessKeyId, string accessKeySecret)
        {
            try
            {
                var config = new ConfigData
                {
                    AccessKeyId = accessKeyId,
                    AccessKeySecret = ProtectString(accessKeySecret)
                };

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置失败: {ex.Message}");
            }
        }

        public (string AccessKeyId, string AccessKeySecret) LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                    return ("", "");

                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<ConfigData>(json);

                if (config == null)
                    return ("", "");

                return (
                    config.AccessKeyId ?? "",
                    UnprotectString(config.AccessKeySecret ?? "")
                );
            }
            catch
            {
                return ("", "");
            }
        }

        public void ClearConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                    File.Delete(_configPath);
            }
            catch
            {
                // 忽略删除错误
            }
        }

        private string ProtectString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            try
            {
                var data = Encoding.UTF8.GetBytes(input);
                var protectedData = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(protectedData);
            }
            catch
            {
                return input; // 如果加密失败，返回原文
            }
        }

        private string UnprotectString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            try
            {
                var protectedData = Convert.FromBase64String(input);
                var data = ProtectedData.Unprotect(protectedData, _entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return input; // 如果解密失败，返回原文
            }
        }

        private class ConfigData
        {
            public string? AccessKeyId { get; set; }
            public string? AccessKeySecret { get; set; }
        }
    }
}
