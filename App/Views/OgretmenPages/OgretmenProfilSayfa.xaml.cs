using App.Models;
using App.Views.Login;
using App.Views.ProfileLyoutMenuViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.OgretmenPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OgretmenProfilSayfa : FlyoutPage
    {
        
        public OgretmenProfilSayfa()
        {
            InitializeComponent();

            flyout.listviewFly.ItemSelected += OnSelectedItem;
        }

        private void OnSelectedItem(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as FlyoutItemPage;
            if(item != null)
            {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetPage));
                flyout.listviewFly.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}