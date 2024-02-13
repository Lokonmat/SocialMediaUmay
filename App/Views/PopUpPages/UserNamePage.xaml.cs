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
	public partial class UserNamePage : Popup
	{
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string mail = Preferences.Get("Mail", "");
        string phoneNumber = Preferences.Get("PhoneNumber", "");
        string tableName = Preferences.Get("Type", "");
        public UserNamePage ()
		{
			InitializeComponent ();

            reNameEntry.Placeholder = fullName;
        }

        private void SaveAndClose_Clicked(object sender, EventArgs e)
        {
            string reFullName = reNameEntry.Text;
            WebServis.TestKBM($"UPDATE `{tableName}` SET `FullName`= '{reFullName}' WHERE `Id`= {userId}");
            WebServis.TestKBM($"UPDATE `paylasim` SET `kullanici_adi`= '{reFullName}' WHERE `kullanici_id`= {userId}");
            Preferences.Set("FullName", reFullName);

            Dismiss(null);
        }

        private void Close_Clicked(object sender, EventArgs e)
        {
            Dismiss(null);
        }
    }
}