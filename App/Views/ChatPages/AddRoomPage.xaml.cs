using App.DataBase;
using App.Models;
using App.ViewModels;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.ChatPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddRoomPage : ContentPage
	{
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        private OgrenciSinif selectedOgrenci;
        public AddRoomPage ()
		{
			InitializeComponent ();
            BindingContext = new AddRoomViewModel();
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem !=  null)
            {
                selectedOgrenci = (OgrenciSinif)e.SelectedItem;
                var db = new DBFireBase();

                ChatRoom newRoom = new ChatRoom
                {
                    Name = selectedOgrenci.Ogrenci_Id.ToString(),
                    Ogrenci_Ad = selectedOgrenci.Ogrenci_Ad.ToString(),
                    Ogrenci_Id = selectedOgrenci.Ogrenci_Id.ToString(),
                    Ogretmen_Id = userId,
                    Ogretmen_Ad = fullName,
                    Sinif_Ad = selectedOgrenci.Sinif_Ad.ToString(),
                };
                List<ChatRoom> existingRooms = await db.GetRoom();
                if(!await IsRoomExists(existingRooms, newRoom))
                {
                    await db.SaveRoom(newRoom);
                }
                await Navigation.PushModalAsync(new MessagesPage());
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