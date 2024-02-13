using App.DataBase;
using App.Models;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.ChatPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ParentToTeacherChatPage : ContentPage
    {
        DBFireBase db = new DBFireBase();
        ChatRoom rm = new ChatRoom();
        string ogretmenAd = Preferences.Get("MesajOgretmenAd", "");
        string fullName = Preferences.Get("FullName", "");
        public ParentToTeacherChatPage()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<MessagesPageParent, ChatRoom>(this, "RoomProp", (page, data) =>
            {
                rm = data;
                collectionViewChat.BindingContext = db.SubChatParent(data.Key);                
                MessagingCenter.Unsubscribe<MessagesPageParent, ChatRoom>(this, "RoomProp");               
            });
            usernamelabel.Text = ogretmenAd;
        }
        public async void Button_Clicked(object sender, EventArgs e)
        {
            var chatOBJ = new Chat { Message = MessageEntry.Text, UserName = fullName };
            await db.SaveMessageParent(chatOBJ, rm.Key);
            MessageEntry.Text = string.Empty;
        }
    }
}