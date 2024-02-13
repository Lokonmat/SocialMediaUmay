using App.Views.PopUpPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.ProfileLyoutMenuViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfileSetting : ContentPage
    {
        public ProfileSetting()
        {
            InitializeComponent();
        }

        private void UserName_Clicked(object sender, EventArgs e)
        {
            Navigation.ShowPopup(new UserNamePage()
            {
                IsLightDismissEnabled = false
            });
        }

        private void Password_Clicked(object sender, EventArgs e)
        {
            Navigation.ShowPopup(new PasswordPage()
            {
                IsLightDismissEnabled = false
            });
        }

        private void Mail_Clicked(object sender, EventArgs e)
        {
            Navigation.ShowPopup(new ChangeMailPage()
            {
                IsLightDismissEnabled = false
            });
        }
    }
}