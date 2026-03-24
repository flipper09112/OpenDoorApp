using Android.App;
using AndroidX.Car.App;
using AndroidX.Car.App.Validation;

namespace OpenDoorApp.UI.Auto
{
    [Service(
        Name = "com.companyname.opendoorapp.OpenDoorCarAppService",
        Exported = true,
        Label = "@string/app_name")]
    [IntentFilter(
        new[] { "androidx.car.app.CarAppService" },
        Categories = new[] { "androidx.car.app.category.POI" })]
    public class OpenDoorCarAppService : CarAppService
    {
        public override HostValidator CreateHostValidator()
        {
            // AllowAllHostsValidator is appropriate for development and personal projects.
            // For production distribution on Google Play, restrict to trusted hosts using
            // HostValidator.Builder with an allowlist XML resource.
            return HostValidator.AllowAllHostsValidator;
        }

        public override Session OnCreateSession()
        {
            return new OpenDoorSession();
        }
    }
}
