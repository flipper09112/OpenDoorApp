using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Autofac;
using OpenDoorApp.Services.Interfaces;
using OpenDoorApp.UI.Fragments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OpenDoorApp.UI.Widget
{
    [BroadcastReceiver(Label = "SimpleAppWidget")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
    public class SimpleAppWidget : AppWidgetProvider
    {
        public static string ACTION_CONNECTING = "ACTION_CONNECTING";
        public static string ACTION_CONNECTED = "ACTION_CONNECTED";
        public static string ACTION_UPDATE = "ACTION_UPDATE";
        public static string ACTION_OPEN_DOOR = "ACTION_OPEN_DOOR";
        private const int MAX_FAIL_CONNECT_TIMES = 6;
        private Context _context;
        private int _failTimes = 0;

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            // There may be multiple widgets active, so update all of them
            foreach (int appWidgetId in appWidgetIds)
            {
                UpdateAppWidget(context, appWidgetManager, appWidgetId);
            }
        }

        private void UpdateAppWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
        {
            // Construct the RemoteViews object
            RemoteViews views = new RemoteViews(context.PackageName, Resource.Layout.simple_app_widget);

            // Construct an Intent which is pointing this class.
            Intent intent = new Intent(context, typeof(SimpleAppWidget));
            intent.SetAction(ACTION_CONNECTING);
            // And this time we are sending a broadcast with getBroadcast
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            views.SetOnClickPendingIntent(Resource.Id.connectButton, pendingIntent);

            // Instruct the widget manager to update the widget
            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            _context = context;

            if (ACTION_CONNECTING.Equals(intent.Action))
            {
                // Construct the RemoteViews object
                RemoteViews views = new RemoteViews(context.PackageName, Resource.Layout.widget_loading);

                // This time we dont have widgetId. Reaching our widget with that way.
                ComponentName appWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(SimpleAppWidget)).Name);
                AppWidgetManager appWidgetManager = AppWidgetManager.GetInstance(context);

                int[] appWidgetIds = appWidgetManager.GetAppWidgetIds(appWidget);

                var _btService = App.Container.Resolve<IBluetoothService>();
                Task.Run(() => { 
                    _btService.Ping(_context, Preferences.Get(HomepageFragment.LastDeviceSelected, string.Empty), UpdateConnectedInfo, SomethingWrong, ShowDataReceived); 
                });

                foreach (int appWidgetId in appWidgetIds)
                {
                    // Instruct the widget manager to update the widget
                    appWidgetManager.UpdateAppWidget(appWidgetId, views);
                }
            }
            else if (ACTION_UPDATE.Equals(intent.Action))
            {
                // This time we dont have widgetId. Reaching our widget with that way.
                ComponentName appWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(SimpleAppWidget)).Name);
                AppWidgetManager appWidgetManager = AppWidgetManager.GetInstance(context);
                int[] appWidgetIds = appWidgetManager.GetAppWidgetIds(appWidget);

                OnUpdate(context, appWidgetManager, appWidgetIds);
            }

            else if (ACTION_CONNECTED.Equals(intent.Action))
            {
                _context = context;
                // Construct the RemoteViews object
                RemoteViews views = new RemoteViews(context.PackageName, Resource.Layout.widget_bt_connected);

                // Construct an Intent which is pointing this class.
                Intent openIntent = new Intent(context, typeof(SimpleAppWidget));
                intent.SetAction(ACTION_OPEN_DOOR);

                // And this time we are sending a broadcast with getBroadcast
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
                views.SetOnClickPendingIntent(Resource.Id.connectButton, pendingIntent);

                // This time we dont have widgetId. Reaching our widget with that way.
                ComponentName appWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(SimpleAppWidget)).Name);
                AppWidgetManager appWidgetManager = AppWidgetManager.GetInstance(context);
                int[] appWidgetIds = appWidgetManager.GetAppWidgetIds(appWidget);

                foreach (int appWidgetId in appWidgetIds)
                {
                    // Instruct the widget manager to update the widget
                    appWidgetManager.UpdateAppWidget(appWidgetId, views);
                }
            }

            else if (ACTION_OPEN_DOOR.Equals(intent.Action))
            {
                _context = context;

                var _btService = App.Container.Resolve<IBluetoothService>();
                Task.Run(() => {
                    _btService.SendCommand(_btService.OpenCommand);
                });

                // Construct the RemoteViews object
                RemoteViews views = new RemoteViews(context.PackageName, Resource.Layout.widget_loading);

                // This time we dont have widgetId. Reaching our widget with that way.
                ComponentName appWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(SimpleAppWidget)).Name);
                AppWidgetManager appWidgetManager = AppWidgetManager.GetInstance(context);

                int[] appWidgetIds = appWidgetManager.GetAppWidgetIds(appWidget);

                Task.Run(async () => {
                    await Task.Delay(10000);
                    RestartLayout();
                });

                foreach (int appWidgetId in appWidgetIds)
                {
                    // Instruct the widget manager to update the widget
                    appWidgetManager.UpdateAppWidget(appWidgetId, views);
                }
            }
        }

        private void RestartLayout()
        {
            Intent intent = new Intent(_context, typeof(SimpleAppWidget));
            intent.SetAction(ACTION_UPDATE);
            OnReceive(_context, intent);
        }

        private void ShowDataReceived(string obj)
        {
        }

        private void SomethingWrong()
        {
            RestartLayout();
        }

        private void UpdateConnectedInfo(bool obj)
        {
            if (obj)
            {
                Intent intent = new Intent(_context, typeof(SimpleAppWidget));
                intent.SetAction(ACTION_CONNECTED);
                OnReceive(_context, intent);
            }
            else
            {
                _failTimes++;
                if(_failTimes > MAX_FAIL_CONNECT_TIMES)
                {
                    RestartLayout();
                    _failTimes = 0;

                    var _btService = App.Container.Resolve<IBluetoothService>();
                    _btService.Stop();
                }
            }
        }
    }
}