using App.DataBase;
using App.Models;
using Plugin.FirebasePushNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.ChatPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessagesPageParent : ContentPage
    {
        DBFireBase db = new DBFireBase();

        public MessagesPageParent()
        {
            InitializeComponent();
            string token = CrossFirebasePushNotification.Current.Token;
            if (token == "")
            {
                DisplayAlert("Bildirimler", "Servis sağlayıcıda yaşanan bir sorundan dolayı bildirim sistemi devre dışı", "Tamam");
            }
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var list = await db.GetRoomParent();
            list.Reverse();
            RoomList.BindingContext = list;
        }
        private void addRoom_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new AddRoomParent());
        }

        private async void RoomRefreshing(object sender, EventArgs e)
        {
            RoomList.BindingContext = await db.GetRoomParent();
            RoomList.IsRefreshing = false;
        }

        private void RoomList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (RoomList.SelectedItem != null)
            {
                var selectRoom = (ChatRoom)RoomList.SelectedItem;
                string ogretmenAd = selectRoom.Ogretmen_Ad;
                Preferences.Set("MesajOgretmenAd", ogretmenAd);
                Navigation.PushModalAsync(new ParentToTeacherChatPage());
                MessagingCenter.Send<MessagesPageParent, ChatRoom>(this, "RoomProp", selectRoom);
            }
        }
    }
}