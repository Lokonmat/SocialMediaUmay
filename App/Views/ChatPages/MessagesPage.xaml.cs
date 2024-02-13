using App.DataBase;
using App.Models;
using Plugin.FirebasePushNotification;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.ChatPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessagesPage : ContentPage
    {

        DBFireBase db = new DBFireBase();
        public MessagesPage()
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
            var list = await db.GetRoom();
            list.Reverse();
            RoomList.BindingContext = list;
        }

        private void addRoom_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new AddRoomPage());
        }

        private async void RoomRefreshing(object sender, EventArgs e)
        {
            RoomList.BindingContext = await db.GetRoom();
            RoomList.IsRefreshing = false;
        }

        private void RoomList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(RoomList.SelectedItem != null)
            {
                var selectRoom = (ChatRoom)RoomList.SelectedItem;
                string ogrenciAd = selectRoom.Ogrenci_Ad;
                Preferences.Set("MesajOgrenciAd", ogrenciAd);
                Navigation.PushModalAsync(new TeacherToParentChatPage());
                MessagingCenter.Send<MessagesPage, ChatRoom>(this, "RoomProp", selectRoom);
            }
        }
    }
}