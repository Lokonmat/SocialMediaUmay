using App.DataBase;
using App.Models;
using App.ViewModels;
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
    public partial class AddRoomParent : ContentPage
    {
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        private OgrenciSinif selectedOgretmen;
        public AddRoomParent()
        {
            InitializeComponent();
            BindingContext = new AddRoomParentViewModel();
        }
        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                selectedOgretmen = (OgrenciSinif)e.SelectedItem;
                var db = new DBFireBase();
                int ogretmenId = selectedOgretmen.Ogretmen_Id;
                string ogretmenAd = selectedOgretmen.Ogretmen_Ad;

                ChatRoom newRoom = new ChatRoom
                {
                    Name = selectedOgretmen.Ogrenci_Id.ToString(),
                    Ogrenci_Ad = selectedOgretmen.Ogrenci_Ad,
                    Ogrenci_Id = selectedOgretmen.Ogrenci_Id.ToString(),
                    Ogretmen_Id = ogretmenId.ToString(),
                    Ogretmen_Ad = ogretmenAd,
                    Sinif_Ad = selectedOgretmen.Sinif_Ad
                };

                List<ChatRoom> existingRooms = await db.GetRoomParent();

                if (!await IsRoomExists(existingRooms, newRoom))
                {
                    await db.SaveRoomParent(newRoom);
                }

                await Navigation.PushModalAsync(new MessagesPageParent());
            }
        }
        public async Task<bool> IsRoomExists(List<ChatRoom> rooms, ChatRoom newRoom)
        {
            // Kontrol edilen özelliklerin aynı olup olmadığını burada belirtin.
            // Örneğin, Name ve Ogrenci_Ad özelliklerini karşılaştırıyoruz.
            return rooms.Any(existingRoom => existingRoom.Name == newRoom.Name && existingRoom.Ogrenci_Ad == newRoom.Ogrenci_Ad && existingRoom.Ogretmen_Id == newRoom.Ogretmen_Id);
        }
    }
}