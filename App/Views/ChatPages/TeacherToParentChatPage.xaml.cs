using App.DataBase;
using App.Models;
using App.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.ChatPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TeacherToParentChatPage : ContentPage
    {
        DBFireBase db = new DBFireBase();
        ChatRoom rm = new ChatRoom();
        string ogrenciAd = Preferences.Get("MesajOgrenciAd", "");
        string fullName = Preferences.Get("FullName", "");
        public TeacherToParentChatPage()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<MessagesPage, ChatRoom>(this, "RoomProp", (page, data) =>
            {
                rm = data;
                collectionViewChat.BindingContext = db.SubChat(data.Key);
                MessagingCenter.Unsubscribe<MessagesPage, ChatRoom>(this, "RoomProp");
            });
            usernamelabel.Text = ogrenciAd;
        }
        public async void Button_Clicked(object sender, EventArgs e)
        {
            string messageText = MessageEntry.Text; // Kullanıcının girdisi

            var chatOBJ = new Chat { Message = messageText, UserName = fullName };
            await db.SaveMessage(chatOBJ, rm.Key);
            MessageEntry.Text = string.Empty;
        }
        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();

        //    scrollView.ScrollToAsync(collectionViewChat, ScrollToPosition.End, false);
        //}
    }
}