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
    public partial class ChangeMailPage : Popup
    {
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string mail = Preferences.Get("Mail", "");
        string phoneNumber = Preferences.Get("PhoneNumber", "");
        string tableName = Preferences.Get("Type", "");

        public ChangeMailPage()
        {
            InitializeComponent();

            reMailEntry.Placeholder = mail;
        }
        private async void SaveAndClose_Clicked(object sender, EventArgs e)
        {
            
            string reMail = reMailEntry.Text;
            WebServis.TestKBM($"UPDATE `{tableName}` SET `Mail`= '{reMail}' WHERE `Id`= {userId}");
            Preferences.Set("Mail", reMail);

            Dismiss(null);
        }

        private void Close_Clicked(object sender, EventArgs e)
        {
            Dismiss(null);
        }
    }
}