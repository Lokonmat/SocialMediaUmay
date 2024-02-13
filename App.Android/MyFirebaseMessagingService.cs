using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Droid
{
    [Service(Enabled = true, Exported = false, Name = "App.Android.MyFirebaseMessagingService")]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            if (message.Data != null)
            {
                bildirimGoster(message); // Bildirimi göster
            }
        }

        private void bildirimGoster(RemoteMessage message)
        {
            string title = message.GetNotification()?.Title;
            string body = message.GetNotification()?.Body;

            var notificationBuilder = new NotificationCompat.Builder(this, "FirebasePushNotificationChannel")
                .SetSmallIcon(Resource.Drawable.icon_home)
                .SetContentTitle(title) // Title buraya ekleniyor
                .SetContentText(body)   // Body buraya ekleniyor
                .SetAutoCancel(false);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(0, notificationBuilder.Build());
        }
    }
}