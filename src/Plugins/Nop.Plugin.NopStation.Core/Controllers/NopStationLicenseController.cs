using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.NopStation.Core.Filters;
using Nop.Plugin.NopStation.Core.Infrastructure;
using Nop.Plugin.NopStation.Core.Models;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.NopStation.Core.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class NopStationLicenseController : BasePluginController
    {
        private readonly IStoreContext _storeContext;
        private readonly INopStationLicenseService _licenseService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        public NopStationLicenseController(IStoreContext storeContext,
            INopStationLicenseService licenseService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _storeContext = storeContext;
            _licenseService = licenseService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        public async Task<IActionResult> License()
        {
            if (!await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageLicense))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var model = new LicenseModel();
            model.ActiveStoreScopeConfiguration = storeId;

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> License(LicenseModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageLicense))
                return AccessDeniedView();

            var result = _licenseService.VerifyProductKey(model.LicenseString);

            switch (result)
            {
                case KeyVerificationResult.InvalidProductKey:
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidProductKey"));
                    return View(model);
                case KeyVerificationResult.InvalidForDomain:
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidForDomain"));
                    return View(model);
                case KeyVerificationResult.InvalidForNOPVersion:
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidForNOPVersion"));
                    return View(model);
                case KeyVerificationResult.Valid:
                    var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                    var settings = await _settingService.LoadSettingAsync<NopStationCoreSettings>(storeId);

                    settings.LicenseStrings.Add(model.LicenseString);
                    await _settingService.SaveSettingAsync(settings);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.Saved"));

                    return RedirectToAction("License");
                default:
                    return RedirectToAction("License");
            }
        }
    }
}
