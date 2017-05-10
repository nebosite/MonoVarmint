using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace DemoForAndroid
{
    [Activity(Label = "DemoForAndroid"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.FullUser
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var g = new AndroidGameRunner();
            g.SoftExit = () => MoveTaskToBack(true);
            SetContentView(g.GetViewService());
            g.Run();
        }
    }
}

