using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.NopStation.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Security;

namespace Nop.Plugin.NopStation.Core.Services
{
    public class NopStationLicenseService : INopStationLicenseService
    {
        private bool? _cachedLicensed = null;

        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEncryptionService _encryptionService;
        private readonly NopStationCoreSettings _coreSettings;

        public static class Constants
        {
            public static string LicenseKeySeed = "22cerfdZX8Uq9LrLHHhYssVD";
        }

        public NopStationLicenseService(ILogger logger,
            IHttpContextAccessor httpContextAccessor,
            IEncryptionService encryptionService,
            NopStationCoreSettings coreSettings)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _encryptionService = encryptionService;
            _coreSettings = coreSettings;
        }

        private DecryptedLicense DecryptProductKey(string productKey, string encryptionKey)
        {
            try
            {
                var decryptedText = _encryptionService.DecryptText(productKey, encryptionKey);

                var decryptedKey = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedText);
                if (decryptedKey == null)
                {
                    return null;
                }

                decryptedKey.TryGetValue("ValidationDateUtc", out string validationDateUtc);
                if (!string.IsNullOrWhiteSpace(validationDateUtc))
                {
                    var dateTime = DateTime.ParseExact(validationDateUtc, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    if (DateTime.UtcNow > dateTime)
                        return null;
                }

                var result = new DecryptedLicense();

                decryptedKey.TryGetValue("NOPVersion", out string nopVersion);
                decryptedKey.TryGetValue("Domain", out string domain);

                result.NopVersion = ExtractVersionComponents(nopVersion);
                result.Domain = domain;

                result.IncludesSubdomains = decryptedKey.ContainsKey("IncludesSubdomains")
                    && (decryptedKey["IncludesSubdomains"].ToLower() == "true");

                return result;
            }
            catch (Exception ex)
            {
                _logger.InformationAsync($"Failed to decrypt nop-station license product key: {ex.Message}", ex).Wait();
            }

            return null;
        }

        private int[] ExtractVersionComponents(string version)
        {
            if (version == null)
                return null;

            var parts = version.Split('.');
            int major = 0, minor = 0;

            try
            {
                major = Convert.ToInt32(parts[0]);
            }
            catch { }
            try
            {
                minor = (int)(Convert.ToInt32(parts[1]) / Math.Pow(10, parts[1].Length - 1));
            }
            catch { }

            return new int[] { major, minor };
        }

        public KeyVerificationResult VerifyProductKey(string key)
        {
            try
            {
                var host = _httpContextAccessor.HttpContext.Request.Host.Host;

                var decryptedKey = DecryptProductKey(key, Constants.LicenseKeySeed);
                if (decryptedKey == null)
                {
                    return KeyVerificationResult.InvalidProductKey;
                }

                if (decryptedKey.NopVersion != null)
                {
                    var currentVersion = ExtractVersionComponents(NopVersion.CURRENT_VERSION);
                    if (currentVersion[0] != decryptedKey.NopVersion[0] || currentVersion[1] != decryptedKey.NopVersion[1])
                    {
                        return KeyVerificationResult.InvalidForNOPVersion;
                    }
                }

                if (host.StartsWith("www."))
                    host = host.Substring(4, host.Length - 4);

                if (decryptedKey.Domain.StartsWith("www."))
                    decryptedKey.Domain = decryptedKey.Domain.Substring(4, host.Length - 4);

                if ((!decryptedKey.IncludesSubdomains && host != decryptedKey.Domain) ||
                    (decryptedKey.IncludesSubdomains && !host.EndsWith($".{decryptedKey.Domain}") &&
                    host != decryptedKey.Domain))
                    return KeyVerificationResult.InvalidForDomain;

                return KeyVerificationResult.Valid;
            }
            catch (Exception ex)
            {
                _logger.ErrorAsync(ex.Message, ex).Wait();
            }

            return KeyVerificationResult.InvalidProductKey;
        }

        public bool IsLicensed()
        {
            if (_cachedLicensed.HasValue)
                return _cachedLicensed.Value;

            foreach (var license in _coreSettings.LicenseStrings)
            {
                if (VerifyProductKey(license) == KeyVerificationResult.Valid)
                {
                    _cachedLicensed = true;
                    break;
                }
            }

            return _cachedLicensed.HasValue ? _cachedLicensed.Value : false;
        }

        #region Inner class

        private class DecryptedLicense
        {
            public int[] NopVersion { get; set; }

            public string Domain { get; set; }

            public bool IncludesSubdomains { get; set; }
        }

        #endregion
    }
}
