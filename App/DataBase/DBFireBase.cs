using App.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace App.DataBase
{
    public class DBFireBase
    {
        string userId = Preferences.Get("Id", "");
        string ogrenci_Id = Preferences.Get("Ogrenci_Id", "");
        int ogretmen_id = int.Parse(Preferences.Get("Ogretmen_Id", "0"));
        FirebaseClient client;

        public DBFireBase()
        {
            client = new FirebaseClient("https://umayapp-c49b6-default-rtdb.europe-west1.firebasedatabase.app/");
        }
        public async Task<List<ChatRoom>> GetRoom()
        {
            return (await client
                .Child($"SocialMediaChat/{userId}")
                .OnceAsync<ChatRoom>())
                .Select(r => new ChatRoom
                {
                    Key = r.Key,
                    Name = r.Object.Name,
                    Ogrenci_Ad = r.Object.Ogrenci_Ad,
                    Ogrenci_Id = r.Object.Ogrenci_Id,
                    Ogretmen_Ad = r.Object.Ogretmen_Ad,
                    Ogretmen_Id = r.Object.Ogretmen_Id,
                    Sinif_Ad = r.Object.Sinif_Ad
                })
                .ToList();
        }
        public async Task<List<ChatRoom>> GetRoomParent()
        {
            var rooms = (await client
                .Child($"SocialMediaChat/{ogretmen_id}")
                .OnceAsync<ChatRoom>())
                .Select(r => new ChatRoom
                {
                    Key = r.Key,
                    Name = r.Object.Name,
                    Ogrenci_Ad = r.Object.Ogrenci_Ad,
                    Ogrenci_Id = r.Object.Ogrenci_Id,
                    Ogretmen_Ad = r.Object.Ogretmen_Ad,
                    Ogretmen_Id = r.Object.Ogretmen_Id,
                    Sinif_Ad = r.Object.Sinif_Ad,
                })
                .Where(room => room.Ogrenci_Id == $"{ogrenci_Id}") // Buraya aranan ogrenci_id eklenmeli
                .ToList();

            return rooms;
        }
        public async Task SaveRoom(ChatRoom rm)
        {
            await client.Child($"SocialMediaChat/{userId}")
                        .PostAsync(rm);
        }
        public async Task SaveRoomParent(ChatRoom rm)
        {
            await client.Child($"SocialMediaChat/{ogretmen_id}")
                        .PostAsync(rm);
        }

        public async Task SaveMessage(Chat ch, string _rm)
        {
            await client.Child($"SocialMediaChat/{userId}/" + _rm + "/Message")
                .PostAsync(ch);
        }
        public async Task SaveMessageParent(Chat ch, string _rm)
        {
            await client.Child($"SocialMediaChat/{ogretmen_id}/" + _rm + "/Message")
                .PostAsync(ch);
        }

        public ObservableCollection<Chat> SubChat(string _roomKey)
        {
            return client.Child($"SocialMediaChat/{userId}/" + _roomKey + "/Message").AsObservable<Chat>().AsObservableCollection<Chat>();
        }
        public ObservableCollection<Chat> SubChatParent(string _roomKey)
        {
            return client.Child($"SocialMediaChat/{ogretmen_id}/" + _roomKey + "/Message").AsObservable<Chat>().AsObservableCollection<Chat>();
        }
    }
}
