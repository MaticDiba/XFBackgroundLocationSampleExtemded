using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XFBackgroundLocationSample.Droid
{
    [Service(Enabled = true)]
    public class TestService : Service
    {
        private Handler handler;
        private Action runnable;
        private bool IsStarted;
        private int DELAY_BETWEEN_LOG_MESSAGES = 5000;
        private int NOTIFICATION_SERVICE_ID = 1001;
        CancellationTokenSource _cts;
        GetLocationService locationService;
        public override void OnCreate()
        {
            _cts = new CancellationTokenSource();
            locationService = new GetLocationService();
            base.OnCreate();
            handler = new Handler(Application.MainLooper);
            runnable = new Action(async () =>
            {
                if (IsStarted)
                {
                    await functionMethod();
                    handler.PostDelayed(runnable, DELAY_BETWEEN_LOG_MESSAGES);
                }
            });
        }
        private async Task functionMethod()
        {
            if(locationService != null)
            {
                await locationService.GetLocation();
            }
        }
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (IsStarted)
            {
                //service is already started
            }
            else
            {
                Notification notification = new NotificationHelper().GetServiceStartedNotification();
                StartForeground(NOTIFICATION_SERVICE_ID, notification);
                handler.PostDelayed(runnable, DELAY_BETWEEN_LOG_MESSAGES);
                IsStarted = true;
            }
            return StartCommandResult.Sticky;
        }
        public override void OnDestroy()
        {
            // Stop the handler.
            handler.RemoveCallbacks(runnable);
            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(NOTIFICATION_SERVICE_ID);
            IsStarted = false;
            if (_cts != null)
            {
                _cts.Token.ThrowIfCancellationRequested();
                _cts.Cancel();
            }
            base.OnDestroy();
        }
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        
    }
}