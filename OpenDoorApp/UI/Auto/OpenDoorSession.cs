using Android.Content;
using AndroidX.Car.App;

namespace OpenDoorApp.UI.Auto
{
    public class OpenDoorSession : Session
    {
        public override Screen OnCreateScreen(Intent intent)
        {
            return new OpenDoorScreen(CarContext);
        }
    }
}
