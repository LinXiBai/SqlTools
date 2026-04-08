using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace CoreToolkit.Common
{
    /// <summary>
    /// 机器码信息（用于序列化）
    /// </summary>
    public class MachineCodeInfo
    {
        /// <summary>
        /// CPU 序列号
        /// </summary>
        public string CpuId { get; set; }

        /// <summary>
        /// 主板序列号
        /// </summary>
        public string MotherboardId { get; set; }

        /// <summary>
        /// 硬盘序列号
        /// </summary>
        public string DiskId { get; set; }

        /// <summary>
        /// 网卡 MAC 地址
        /// </summary>
        public string MacAddress { get; set; }

        /// <summary>
        /// 系统安装日期
        /// </summary>
        public DateTime? InstallDate { get; set; }

        /// <summary>
        /// 计算机名
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OsVersion { get; set; }

        /// <summary>
        /// 扩展信息（自定义字段）
        /// </summary>
        public string ExtraInfo { get; set; }

        /// <summary>
        /// 生成时间戳
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 授权码信息（用于序列化）
    /// </summary>
    public class LicenseCodeInfo
    {
        /// <summary>
        /// 授权类型（试用/正式/永久）
        /// </summary>
        public LicenseType Type { get; set; }

        /// <summary>
        /// 授权开始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 授权结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 授权天数（用于计算 EndDate）
        /// </summary>
        public int? ValidDays { get; set; }

        /// <summary>
        /// 授权功能列表（逗号分隔）
        /// </summary>
        public string Features { get; set; }

        /// <summary>
        /// 最大设备数量
        /// </summary>
        public int? MaxDevices { get; set; }

        /// <summary>
        /// 授权版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 授权签名（防篡改）
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// 生成时间戳
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 扩展信息（自定义字段）
        /// </summary>
        public string ExtraInfo { get; set; }
    }

    /// <summary>
    /// 授权类型枚举
    /// </summary>
    public enum LicenseType
    {
        /// <summary>
        /// 试用授权
        /// </summary>
        Trial,

        /// <summary>
        /// 正式授权
        /// </summary>
        Standard,

        /// <summary>
        /// 永久授权
        /// </summary>
        Permanent,

        /// <summary>
        /// 企业授权
        /// </summary>
        Enterprise
    }

    /// <summary>
    /// 授权码序列化/反序列化辅助类
    /// </summary>
    public static class LicenseSerializer
    {
        private static readonly string DefaultEncryptionKey = "LicenseDefaultKey2024";

        #region 机器码序列化

        /// <summary>
        /// 将机器码信息序列化为 JSON 字符串
        /// </summary>
        public static string SerializeMachineCode(MachineCodeInfo machineCode)
        {
            if (machineCode == null)
                throw new ArgumentNullException(nameof(machineCode));

            return JsonConvert.SerializeObject(machineCode, Formatting.None);
        }

        /// <summary>
        /// 将机器码信息序列化为 JSON 字符串并加密
        /// </summary>
        public static string SerializeMachineCodeEncrypted(MachineCodeInfo machineCode, string key = null)
        {
            string json = SerializeMachineCode(machineCode);
            return Encrypt(json, key ?? DefaultEncryptionKey);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化机器码信息
        /// </summary>
        public static MachineCodeInfo DeserializeMachineCode(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<MachineCodeInfo>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从加密字符串解密并反序列化机器码信息
        /// </summary>
        public static MachineCodeInfo DeserializeMachineCodeEncrypted(string encryptedData, string key = null)
        {
            if (string.IsNullOrEmpty(encryptedData))
                return null;

            try
            {
                string json = Decrypt(encryptedData, key ?? DefaultEncryptionKey);
                return DeserializeMachineCode(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 生成机器码信息（从当前系统）
        /// </summary>
        public static MachineCodeInfo GenerateMachineCode(string extraInfo = null)
        {
            var info = new MachineCodeInfo
            {
                MachineName = Environment.MachineName,
                OsVersion = Environment.OSVersion.ToString(),
                GeneratedAt = DateTime.Now,
                ExtraInfo = extraInfo
            };

            try
            {
                // 尝试获取 CPU ID
                info.CpuId = GetCpuId();
            }
            catch { }

            try
            {
                // 尝试获取 MAC 地址
                info.MacAddress = GetMacAddress();
            }
            catch { }

            try
            {
                // 尝试获取硬盘序列号
                info.DiskId = GetDiskId();
            }
            catch { }

            return info;
        }

        /// <summary>
        /// 生成机器码指纹（用于比对）
        /// </summary>
        public static string GenerateMachineFingerprint(MachineCodeInfo machineCode)
        {
            if (machineCode == null)
                return string.Empty;

            // 组合关键硬件信息生成指纹
            var components = new[]
            {
                machineCode.CpuId,
                machineCode.MotherboardId,
                machineCode.DiskId,
                machineCode.MacAddress
            };

            var fingerprint = string.Join("|", components);
            return ComputeHash(fingerprint);
        }

        #endregion

        #region 授权码序列化

        /// <summary>
        /// 将授权码信息序列化为 JSON 字符串
        /// </summary>
        public static string SerializeLicenseCode(LicenseCodeInfo licenseCode)
        {
            if (licenseCode == null)
                throw new ArgumentNullException(nameof(licenseCode));

            return JsonConvert.SerializeObject(licenseCode, Formatting.None);
        }

        /// <summary>
        /// 将授权码信息序列化为 JSON 字符串并加密
        /// </summary>
        public static string SerializeLicenseCodeEncrypted(LicenseCodeInfo licenseCode, string key = null)
        {
            string json = SerializeLicenseCode(licenseCode);
            return Encrypt(json, key ?? DefaultEncryptionKey);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化授权码信息
        /// </summary>
        public static LicenseCodeInfo DeserializeLicenseCode(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<LicenseCodeInfo>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从加密字符串解密并反序列化授权码信息
        /// </summary>
        public static LicenseCodeInfo DeserializeLicenseCodeEncrypted(string encryptedData, string key = null)
        {
            if (string.IsNullOrEmpty(encryptedData))
                return null;

            try
            {
                string json = Decrypt(encryptedData, key ?? DefaultEncryptionKey);
                return DeserializeLicenseCode(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 创建授权码信息
        /// </summary>
        public static LicenseCodeInfo CreateLicenseCode(LicenseType type, int validDays, string features = null, 
            string version = "1.0", int? maxDevices = null, string extraInfo = null)
        {
            var info = new LicenseCodeInfo
            {
                Type = type,
                StartDate = DateTime.Now.Date,
                ValidDays = validDays,
                EndDate = type == LicenseType.Permanent ? (DateTime?)null : DateTime.Now.Date.AddDays(validDays),
                Features = features,
                Version = version,
                MaxDevices = maxDevices,
                GeneratedAt = DateTime.Now,
                ExtraInfo = extraInfo
            };

            // 生成签名
            info.Signature = GenerateSignature(info);

            return info;
        }

        /// <summary>
        /// 验证授权码是否有效
        /// </summary>
        public static bool ValidateLicenseCode(LicenseCodeInfo licenseCode, out string errorMessage)
        {
            errorMessage = null;

            if (licenseCode == null)
            {
                errorMessage = "授权码信息为空";
                return false;
            }

            // 验证签名
            string expectedSignature = GenerateSignature(licenseCode);
            if (licenseCode.Signature != expectedSignature)
            {
                errorMessage = "授权码签名无效，可能已被篡改";
                return false;
            }

            // 验证有效期
            if (licenseCode.Type != LicenseType.Permanent && licenseCode.EndDate.HasValue)
            {
                if (DateTime.Now.Date > licenseCode.EndDate.Value)
                {
                    errorMessage = "授权码已过期";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查授权码是否在有效期内
        /// </summary>
        public static bool IsLicenseValid(LicenseCodeInfo licenseCode)
        {
            if (licenseCode == null)
                return false;

            if (licenseCode.Type == LicenseType.Permanent)
                return true;

            if (!licenseCode.EndDate.HasValue)
                return false;

            return DateTime.Now.Date <= licenseCode.EndDate.Value;
        }

        /// <summary>
        /// 获取授权码剩余天数
        /// </summary>
        public static int GetRemainingDays(LicenseCodeInfo licenseCode)
        {
            if (licenseCode == null)
                return 0;

            if (licenseCode.Type == LicenseType.Permanent)
                return int.MaxValue;

            if (!licenseCode.EndDate.HasValue)
                return 0;

            var remaining = licenseCode.EndDate.Value - DateTime.Now.Date;
            return remaining.Days;
        }

        #endregion

        #region 加密/解密

        /// <summary>
        /// 加密字符串
        /// </summary>
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using (var aes = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                byte[] ivBytes = Encoding.UTF8.GetBytes(key.PadRight(16).Substring(0, 16));

                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                using (var aes = Aes.Create())
                {
                    byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                    byte[] ivBytes = Encoding.UTF8.GetBytes(key.PadRight(16).Substring(0, 16));

                    aes.Key = keyBytes;
                    aes.IV = ivBytes;

                    byte[] cipherBytes = Convert.FromBase64String(cipherText);

                    using (var ms = new MemoryStream(cipherBytes))
                    using (var decryptor = aes.CreateDecryptor())
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 生成授权码签名
        /// </summary>
        private static string GenerateSignature(LicenseCodeInfo licenseCode)
        {
            // 组合关键字段生成签名
            var data = $"{licenseCode.Type}|{licenseCode.StartDate:yyyyMMdd}|{licenseCode.EndDate:yyyyMMdd}|{licenseCode.Features}|{licenseCode.Version}|{licenseCode.GeneratedAt:yyyyMMddHHmmss}";
            return ComputeHash(data);
        }

        /// <summary>
        /// 计算字符串的哈希值
        /// </summary>
        private static string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// 获取 CPU ID（简化实现）
        /// </summary>
        private static string GetCpuId()
        {
            try
            {
                var cpuInfo = string.Empty;
                using (var mc = new System.Management.ManagementClass("win32_processor"))
                {
                    var moc = mc.GetInstances();
                    foreach (var mo in moc)
                    {
                        cpuInfo = mo.Properties["processorId"].Value.ToString();
                        break;
                    }
                }
                return cpuInfo;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取 MAC 地址（简化实现）
        /// </summary>
        private static string GetMacAddress()
        {
            try
            {
                var macAddress = string.Empty;
                using (var mc = new System.Management.ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    var moc = mc.GetInstances();
                    foreach (var mo in moc)
                    {
                        if ((bool)mo["IPEnabled"])
                        {
                            macAddress = mo["MacAddress"].ToString();
                            break;
                        }
                    }
                }
                return macAddress;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取硬盘序列号（简化实现）
        /// </summary>
        private static string GetDiskId()
        {
            try
            {
                var diskId = string.Empty;
                using (var mc = new System.Management.ManagementClass("Win32_DiskDrive"))
                {
                    var moc = mc.GetInstances();
                    foreach (var mo in moc)
                    {
                        diskId = mo.Properties["Model"].Value.ToString();
                        break;
                    }
                }
                return diskId;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
