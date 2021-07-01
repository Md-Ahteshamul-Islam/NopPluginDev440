using Nop.Plugin.NopStation.Core.Infrastructure;

namespace Nop.Plugin.NopStation.Core.Services
{
    public interface INopStationLicenseService
    {
        bool IsLicensed();

        KeyVerificationResult VerifyProductKey(string key);
    }
}
