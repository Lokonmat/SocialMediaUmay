using App.Views.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.PopUpPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PasswordPage : Popup
    {
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string mail = Preferences.Get("Mail", "");
        string phoneNumber = Preferences.Get("PhoneNumber", "");
        string tableName = Preferences.Get("Type", "");

        public PasswordPage()
        {
            InitializeComponent();
        }

        private async void SaveAndClose_Clicked(object sender, EventArgs e)
        {
            string query = WebServis.TestKBM($"select `Password`FROM `{tableName}` WHERE `Id`={userId}");
            if (passwordEntry.Text == query)
            {
                string rePassword = rePasswordEntry.Text;
                WebServis.TestKBM($"UPDATE `{tableName}` SET `Password`= '{rePassword}' WHERE `Id`= {userId}");
                Preferences.Set("Password", rePassword);

                App.Current.MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                
            }
           
        }

        private void Close_Clicked(object sender, EventArgs e)
        {
            Dismiss(null);
        }
    }
}