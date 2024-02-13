using App.Views.Login;
using App.Views.OgretmenPages;
using App.Views.VeliPages;
using FirebaseAdmin.Messaging;
using Plugin.FirebasePushNotification;
using System;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App
{
    public partial class App : Application
    {
        readonly string userId = Preferences.Get("Id", "");
        readonly string tableName = Preferences.Get("Type", "");
        public const string ftpServerUrl = "ftp://www.destek88.com/public_html/umayapp/";
        public const string ftpUserName = "destek88";
        public const string ftpPassword = "w2LBhF27";

        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage());
            if (Preferences.ContainsKey("RememberSwitch") && Preferences.Get("RememberSwitch", false))
            {
                if (Preferences.ContainsKey("Type"))
                {
                    string rememberedType = Preferences.Get("Type", string.Empty);

                    // Eğer hatırlanan kullanıcı tipi "ogretmen" ise
                    if (rememberedType.Equals("ogretmen", StringComparison.OrdinalIgnoreCase))
                    {
                        // OgretmenAnaSayfa aç
                        string mail = Preferences.Get("Mail", string.Empty);
                        string password = Preferences.Get("Password", string.Empty);
                        string sorgu = $"select `Mail`, `Password` FROM {tableName} WHERE Mail = '{mail}' AND Password = '{password}'";
                        string kullanici = WebServis.TestKBM(sorgu);
                        if (!string.IsNullOrEmpty(kullanici))
                        {
                            SaveTokenToDatabase(); // Tokeni kaydet
                            App.Current.MainPage = new NavigationPage(new OgretmenAnaSayfa());
                        }
                        return;
                    }
                    // Eğer hatırlanan kullanıcı tipi "veli" ise
                    else if (rememberedType.Equals("veli", StringComparison.OrdinalIgnoreCase))
                    {
                        // VeliAnaSayfa aç
                        string mail = Preferences.Get("Mail", string.Empty);
                        string password = Preferences.Get("Password", string.Empty);
                        string sorgu = $"select `Mail`, `Password` FROM {tableName} WHERE Mail = '{mail}' AND Password = '{password}'";
                        string kullanici = WebServis.TestKBM(sorgu);
                        if (!string.IsNullOrEmpty(kullanici))
                        {
                            SaveTokenToDatabase(); // Tokeni kaydet
                            App.Current.MainPage = new NavigationPage(new VeliAnaSayfa());
                        }
                        return;
                    }
                }
            }
            //CheckInternetConnection();
        }
        private void SaveTokenToDatabase()
        {
            string token = CrossFirebasePushNotification.Current.Token;
            string updateTokenQuery = $"UPDATE `{tableName}` SET `ActiveToken` = '{token}' WHERE `Id` = {userId}";
            WebServis.TestKBM(updateTokenQuery);
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
        //private async void CheckInternetConnection()
        //{            
        //    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        //    {
        //        bool result = await MainPage.DisplayAlert("Uyarı", "İnternet bağlantınız bulunmamakta. İnternete bağlanıp tekrar deneyiniz", "Tamam", "");

        //        if (result)
        //        {
        //            // "Tamam" butonuna basıldığında uygulama kapatılıyor.
        //            System.Diagnostics.Process.GetCurrentProcess().Kill();
        //        }
        //    }
        //}

    }
}
